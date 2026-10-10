using FluentTerminal.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FluentTerminal.Models
{
    /// <summary>
    /// Pure (UI-free) helpers to manage a binary pane tree, Windows Terminal style.
    /// Root is either a <see cref="PaneLeafNode"/> (single pane) or a <see cref="PaneSplitNode"/>.
    /// </summary>
    public static class PaneLayoutHelper
    {
        public static IList<PaneLeafNode> GetLeaves(PaneNode root)
        {
            var result = new List<PaneLeafNode>();
            CollectLeaves(root, result);
            return result;
        }

        private static void CollectLeaves(PaneNode node, List<PaneLeafNode> result)
        {
            if (node == null)
            {
                return;
            }

            if (node is PaneLeafNode leaf)
            {
                result.Add(leaf);
            }
            else if (node is PaneSplitNode split)
            {
                CollectLeaves(split.First, result);
                CollectLeaves(split.Second, result);
            }
        }

        /// <summary>
        /// Splits <paramref name="target"/> by replacing it with a split node
        /// containing the target and <paramref name="newLeaf"/>.
        /// Returns the (possibly new) root.
        /// </summary>
        public static PaneNode Split(PaneNode root, PaneLeafNode target, PaneLeafNode newLeaf, SplitOrientation orientation)
        {
            if (target == null) throw new ArgumentNullException(nameof(target));
            if (newLeaf == null) throw new ArgumentNullException(nameof(newLeaf));
            if (root == null) throw new ArgumentNullException(nameof(root));

            // NB: capture parent BEFORE creating the split node, as the
            // PaneSplitNode ctor re-parents target.
            var parent = target.Parent;

            var split = new PaneSplitNode(orientation, target, newLeaf);

            if (parent == null)
            {
                // target was root
                return split;
            }

            if (ReferenceEquals(parent.First, target))
            {
                parent.First = split;
            }
            else
            {
                parent.Second = split;
            }

            return root;
        }

        /// <summary>
        /// Closes <paramref name="target"/>. Its sibling takes its place.
        /// Returns the new root, or null if it was the last pane.
        /// </summary>
        public static PaneNode Close(PaneNode root, PaneLeafNode target)
        {
            if (target == null) throw new ArgumentNullException(nameof(target));
            if (root == null) throw new ArgumentNullException(nameof(root));

            var parent = target.Parent;
            if (parent == null)
            {
                // Last pane closed
                return null;
            }

            var sibling = ReferenceEquals(parent.First, target) ? parent.Second : parent.First;
            var grandParent = parent.Parent;

            sibling.Parent = null;
            if (grandParent == null)
            {
                return sibling;
            }

            if (ReferenceEquals(grandParent.First, parent))
            {
                grandParent.First = sibling;
            }
            else
            {
                grandParent.Second = sibling;
            }

            return root;
        }

        public static PaneLeafNode FocusNext(PaneNode root, PaneLeafNode current)
        {
            var leaves = GetLeaves(root);
            if (leaves.Count == 0) return null;
            if (current == null) return leaves[0];

            var index = leaves.IndexOf(current);
            if (index < 0) return leaves[0];
            return leaves[(index + 1) % leaves.Count];
        }

        public static PaneLeafNode FocusPrevious(PaneNode root, PaneLeafNode current)
        {
            var leaves = GetLeaves(root);
            if (leaves.Count == 0) return null;
            if (current == null) return leaves[0];

            var index = leaves.IndexOf(current);
            if (index < 0) return leaves[0];
            return leaves[(index - 1 + leaves.Count) % leaves.Count];
        }

        private struct Rect
        {
            public double X, Y, W, H;
            public double CenterX => X + W / 2;
            public double CenterY => Y + H / 2;
        }

        private static void CollectBounds(PaneNode node, Rect bounds, Dictionary<PaneLeafNode, Rect> map)
        {
            if (node is PaneLeafNode leaf)
            {
                map[leaf] = bounds;
            }
            else if (node is PaneSplitNode split)
            {
                // Vertical = side by side (split along X), Horizontal = stacked (split along Y)
                if (split.Orientation == SplitOrientation.Vertical)
                {
                    var w1 = bounds.W * split.Ratio;
                    CollectBounds(split.First, new Rect { X = bounds.X, Y = bounds.Y, W = w1, H = bounds.H }, map);
                    CollectBounds(split.Second, new Rect { X = bounds.X + w1, Y = bounds.Y, W = bounds.W - w1, H = bounds.H }, map);
                }
                else
                {
                    var h1 = bounds.H * split.Ratio;
                    CollectBounds(split.First, new Rect { X = bounds.X, Y = bounds.Y, W = bounds.W, H = h1 }, map);
                    CollectBounds(split.Second, new Rect { X = bounds.X, Y = bounds.Y + h1, W = bounds.W, H = bounds.H - h1 }, map);
                }
            }
        }

        public enum FocusDirection { Up, Down, Left, Right }

        /// <summary>
        /// Directional focus, WT-style: nearest pane strictly in the given direction
        /// based on subdivided layout rectangles.
        /// </summary>
        public static PaneLeafNode MoveFocus(PaneNode root, PaneLeafNode current, FocusDirection direction)
        {
            if (root == null || current == null) return current;

            var map = new Dictionary<PaneLeafNode, Rect>();
            CollectBounds(root, new Rect { X = 0, Y = 0, W = 1, H = 1 }, map);

            if (!map.TryGetValue(current, out var from)) return current;

            PaneLeafNode best = null;
            var bestScore = double.MaxValue;

            foreach (var kv in map)
            {
                if (ReferenceEquals(kv.Key, current)) continue;
                var to = kv.Value;

                var dx = to.CenterX - from.CenterX;
                var dy = to.CenterY - from.CenterY;

                bool inDirection;
                switch (direction)
                {
                    case FocusDirection.Left: inDirection = to.CenterX < from.CenterX - 1e-9; break;
                    case FocusDirection.Right: inDirection = to.CenterX > from.CenterX + 1e-9; break;
                    case FocusDirection.Up: inDirection = to.CenterY < from.CenterY - 1e-9; break;
                    case FocusDirection.Down: inDirection = to.CenterY > from.CenterY + 1e-9; break;
                    default: inDirection = false; break;
                }

                if (!inDirection) continue;

                // Primary axis distance must dominate, secondary axis penalized (WT-like).
                double primary, secondary;
                if (direction == FocusDirection.Left || direction == FocusDirection.Right)
                {
                    primary = Math.Abs(dx);
                    secondary = Math.Abs(dy);
                }
                else
                {
                    primary = Math.Abs(dy);
                    secondary = Math.Abs(dx);
                }

                var score = primary + secondary * 2.0;
                if (score < bestScore)
                {
                    bestScore = score;
                    best = kv.Key;
                }
            }

            return best ?? current;
        }
    }
}
