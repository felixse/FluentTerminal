using FluentTerminal.Models.Enums;
using System;
using System.Collections.Generic;

namespace FluentTerminal.Models
{
    public abstract class PaneNode
    {
        public PaneSplitNode Parent { get; internal set; }
    }

    public sealed class PaneLeafNode : PaneNode
    {
        public PaneLeafNode(int id)
        {
            Id = id;
        }

        public int Id { get; }
    }

    public sealed class PaneSplitNode : PaneNode
    {
        private PaneNode _first;
        private PaneNode _second;

        public PaneSplitNode(SplitOrientation orientation, PaneNode first, PaneNode second, double ratio = 0.5)
        {
            Orientation = orientation;
            Ratio = ratio;
            First = first ?? throw new ArgumentNullException(nameof(first));
            Second = second ?? throw new ArgumentNullException(nameof(second));
        }

        public SplitOrientation Orientation { get; set; }

        public double Ratio { get; set; }

        public PaneNode First
        {
            get => _first;
            set
            {
                _first = value ?? throw new ArgumentNullException(nameof(value));
                _first.Parent = this;
            }
        }

        public PaneNode Second
        {
            get => _second;
            set
            {
                _second = value ?? throw new ArgumentNullException(nameof(value));
                _second.Parent = this;
            }
        }
    }
}
