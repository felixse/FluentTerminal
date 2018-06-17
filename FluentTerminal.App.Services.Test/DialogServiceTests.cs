using System;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using FluentTerminal.App.Services.Dialogs;
using FluentTerminal.App.Services.Implementation;
using Moq;
using Xunit;

namespace FluentTerminal.App.Services.Test
{
    public class DialogServiceTests
    {
        private readonly Fixture _fixture;

        public DialogServiceTests()
        {
            _fixture = new Fixture();
        }

        [Fact]
        public void ShowDialogAsync_TitleIsEmpty_ThrowsArgumentNullException()
        {
            var title = string.Empty;
            var content = _fixture.Create<string>();
            var buttons = _fixture.CreateMany<DialogButton>(2);
            var dialogService = new DialogService(() => Mock.Of<IShellProfileSelectionDialog>(), () => Mock.Of<IMessageDialog>(), () => Mock.Of<ICreateKeyBindingDialog>());

            Func<Task<DialogButton>> invoke = () => dialogService.ShowMessageDialogAsnyc(title, content, buttons.ElementAt(0), buttons.ElementAt(1));

            invoke.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("title");
        }

        [Fact]
        public void ShowDialogAsync_ContentIsEmpty_ThrowsArgumentNullException()
        {
            var title = _fixture.Create<string>();
            var content = string.Empty;
            var buttons = _fixture.CreateMany<DialogButton>(2);
            var dialogService = new DialogService(() => Mock.Of<IShellProfileSelectionDialog>(), () => Mock.Of<IMessageDialog>(), () => Mock.Of<ICreateKeyBindingDialog>());

            Func<Task<DialogButton>> invoke = () => dialogService.ShowMessageDialogAsnyc(title, content, buttons.ElementAt(0), buttons.ElementAt(1));

            invoke.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("content");
        }

        [Fact]
        public void ShowDialogAsync_NoButtonsPassed_ThrowsArgumentException()
        {
            var title = _fixture.Create<string>();
            var content = _fixture.Create<string>();
            var dialogService = new DialogService(() => Mock.Of<IShellProfileSelectionDialog>(), () => Mock.Of<IMessageDialog>(), () => Mock.Of<ICreateKeyBindingDialog>());

            Func<Task<DialogButton>> invoke = () => dialogService.ShowMessageDialogAsnyc(title, content);

            invoke.Should().Throw<ArgumentException>().And.ParamName.Should().Be("buttons");
        }
    }
}
