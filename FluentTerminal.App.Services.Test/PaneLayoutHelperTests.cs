using FluentAssertions;
using FluentTerminal.Models;
using FluentTerminal.Models.Enums;
using Xunit;

namespace FluentTerminal.App.Services.Test
{
    public class PaneLayoutHelperTests
    {
        [Fact]
        public void Split_SinglePane_CreatesTwoLeaves()
        {
            var root = (PaneNode)new PaneLeafNode(1);

            root = PaneLayoutHelper.Split(root, (PaneLeafNode)root, new PaneLeafNode(2), SplitOrientation.Vertical);

            var leaves = PaneLayoutHelper.GetLeaves(root);
            leaves.Should().HaveCount(2);
        }

        [Fact]
        public void Close_OneOfTwoPanes_SiblingBecomesRoot()
        {
            var first = new PaneLeafNode(1);
            PaneNode root = first;
            var second = new PaneLeafNode(2);
            root = PaneLayoutHelper.Split(root, first, second, SplitOrientation.Horizontal);

            root = PaneLayoutHelper.Close(root, first);

            var leaves = PaneLayoutHelper.GetLeaves(root);
            leaves.Should().ContainSingle().Which.Id.Should().Be(2);
        }

        [Fact]
        public void Close_LastPane_ReturnsNull()
        {
            PaneNode root = new PaneLeafNode(1);

            root = PaneLayoutHelper.Close(root, (PaneLeafNode)root);

            root.Should().BeNull();
        }

        [Fact]
        public void FocusNext_CyclesInOrder()
        {
            var a = new PaneLeafNode(1);
            PaneNode root = a;
            var b = new PaneLeafNode(2);
            root = PaneLayoutHelper.Split(root, a, b, SplitOrientation.Vertical);
            var c = new PaneLeafNode(3);
            root = PaneLayoutHelper.Split(root, b, c, SplitOrientation.Horizontal);

            PaneLayoutHelper.FocusNext(root, a).Should().Be(b);
            PaneLayoutHelper.FocusNext(root, b).Should().Be(c);
            PaneLayoutHelper.FocusNext(root, c).Should().Be(a);
        }

        [Fact]
        public void MoveFocus_VerticalSplit_LeftRight()
        {
            var left = new PaneLeafNode(1);
            PaneNode root = left;
            var right = new PaneLeafNode(2);
            root = PaneLayoutHelper.Split(root, left, right, SplitOrientation.Vertical);

            PaneLayoutHelper.MoveFocus(root, left, PaneLayoutHelper.FocusDirection.Right).Should().Be(right);
            PaneLayoutHelper.MoveFocus(root, right, PaneLayoutHelper.FocusDirection.Left).Should().Be(left);
            // No pane above: stays
            PaneLayoutHelper.MoveFocus(root, left, PaneLayoutHelper.FocusDirection.Up).Should().Be(left);
        }

        [Fact]
        public void MoveFocus_HorizontalSplit_UpDown()
        {
            var top = new PaneLeafNode(1);
            PaneNode root = top;
            var bottom = new PaneLeafNode(2);
            root = PaneLayoutHelper.Split(root, top, bottom, SplitOrientation.Horizontal);

            PaneLayoutHelper.MoveFocus(root, top, PaneLayoutHelper.FocusDirection.Down).Should().Be(bottom);
            PaneLayoutHelper.MoveFocus(root, bottom, PaneLayoutHelper.FocusDirection.Up).Should().Be(top);
        }
    }
}
