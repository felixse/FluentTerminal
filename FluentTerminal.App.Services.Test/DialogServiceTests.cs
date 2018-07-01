using System;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using FluentTerminal.App.Services.Dialogs;
using FluentTerminal.App.Services.Implementation;
using FluentTerminal.Models;
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
        public void ShowCreateKeyBindingDialog_Default_UsesCreateKeyBindingDialog()
        {
            var createKeyBindingDialog = new Mock<ICreateKeyBindingDialog>();
            var dialogService = new DialogService(Mock.Of<IShellProfileSelectionDialog>, Mock.Of<IMessageDialog>, () => createKeyBindingDialog.Object);

            dialogService.ShowCreateKeyBindingDialog();

            createKeyBindingDialog.Verify(x => x.CreateKeyBinding(), Times.Once);
        }

        [Fact]
        public void ShowMessageDialogAsnyc_TitleIsEmpty_ThrowsArgumentNullException()
        {
            var title = string.Empty;
            var content = _fixture.Create<string>();
            var buttons = _fixture.CreateMany<DialogButton>(2);
            var dialogService = new DialogService(Mock.Of<IShellProfileSelectionDialog>, Mock.Of<IMessageDialog>, Mock.Of<ICreateKeyBindingDialog>);

            Func<Task<DialogButton>> showMessageDialogAsnyc = () => dialogService.ShowMessageDialogAsnyc(title, content, buttons.ElementAt(0), buttons.ElementAt(1));

            showMessageDialogAsnyc.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("title");
        }

        [Fact]
        public void ShowMessageDialogAsnyc_ContentIsEmpty_ThrowsArgumentNullException()
        {
            var title = _fixture.Create<string>();
            var content = string.Empty;
            var buttons = _fixture.CreateMany<DialogButton>(2);
            var dialogService = new DialogService(Mock.Of<IShellProfileSelectionDialog>, Mock.Of<IMessageDialog>, Mock.Of<ICreateKeyBindingDialog>);

            Func<Task<DialogButton>> showMessageDialogAsnyc = () => dialogService.ShowMessageDialogAsnyc(title, content, buttons.ElementAt(0), buttons.ElementAt(1));

            showMessageDialogAsnyc.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("content");
        }

        [Fact]
        public void ShowMessageDialogAsnyc_NoButtonsPassed_ThrowsArgumentException()
        {
            var title = _fixture.Create<string>();
            var content = _fixture.Create<string>();
            var dialogService = new DialogService(Mock.Of<IShellProfileSelectionDialog>, Mock.Of<IMessageDialog>, Mock.Of<ICreateKeyBindingDialog>);

            Func<Task<DialogButton>> showMessageDialogAsnyc = () => dialogService.ShowMessageDialogAsnyc(title, content);

            showMessageDialogAsnyc.Should().Throw<ArgumentException>().And.ParamName.Should().Be("buttons");
        }

        [Fact]
        public void ShowMessageDialogAsnyc_Default_UsesMessageDialog()
        {
            var title = _fixture.Create<string>();
            var content = _fixture.Create<string>();
            var buttons = _fixture.CreateMany<DialogButton>(2);
            var messageDialog = new Mock<IMessageDialog>();
            var dialogService = new DialogService(Mock.Of<IShellProfileSelectionDialog>, () => messageDialog.Object, Mock.Of<ICreateKeyBindingDialog>);

            dialogService.ShowMessageDialogAsnyc(title, content, buttons.ElementAt(0), buttons.ElementAt(1));

            messageDialog.Verify(x => x.ShowAsync(), Times.Once);
        }

        [Fact]
        public void ShowProfileSelectionDialogAsync_Default_UsesShellProfileSelectionDialog()
        {
            var shellProfileSelectionDialog = new Mock<IShellProfileSelectionDialog>();
            var dialogService = new DialogService(() => shellProfileSelectionDialog.Object, Mock.Of<IMessageDialog>, Mock.Of<ICreateKeyBindingDialog>);

            dialogService.ShowProfileSelectionDialogAsync();

            shellProfileSelectionDialog.Verify(x => x.SelectProfile(), Times.Once);
        }
    }
}
