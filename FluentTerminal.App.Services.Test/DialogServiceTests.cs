using AutoFixture;
using FluentAssertions;
using FluentTerminal.App.Services.Dialogs;
using FluentTerminal.App.Services.Implementation;
using Moq;
using System;
using System.Linq;
using System.Threading.Tasks;
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
            var dialogService = new DialogService(Mock.Of<IMessageDialog>, () => createKeyBindingDialog.Object, Mock.Of<IInputDialog>, Mock.Of<ISshConnectionInfoDialog>, Mock.Of<ICustomCommandDialog>, Mock.Of<IAboutDialog>);

            dialogService.ShowCreateKeyBindingDialog();

            createKeyBindingDialog.Verify(x => x.CreateKeyBinding(), Times.Once);
        }

        [Fact]
        public async Task ShowMessageDialogAsnyc_TitleIsEmpty_ThrowsArgumentNullException()
        {
            var title = string.Empty;
            var content = _fixture.Create<string>();
            var buttons = _fixture.CreateMany<DialogButton>(2);
            var dialogService = new DialogService(Mock.Of<IMessageDialog>, Mock.Of<ICreateKeyBindingDialog>, Mock.Of<IInputDialog>, Mock.Of<ISshConnectionInfoDialog>, Mock.Of<ICustomCommandDialog>, Mock.Of<IAboutDialog>);

            Func<Task<DialogButton>> showMessageDialogAsnyc = () => dialogService.ShowMessageDialogAsync(title, content, buttons.ElementAt(0), buttons.ElementAt(1));

            await showMessageDialogAsnyc.Should().ThrowAsync<ArgumentNullException>().WithParameterName("title");
        }

        [Fact]
        public async Task ShowMessageDialogAsnyc_ContentIsEmpty_ThrowsArgumentNullException()
        {
            var title = _fixture.Create<string>();
            var content = string.Empty;
            var buttons = _fixture.CreateMany<DialogButton>(2);
            var dialogService = new DialogService(Mock.Of<IMessageDialog>, Mock.Of<ICreateKeyBindingDialog>, Mock.Of<IInputDialog>, Mock.Of<ISshConnectionInfoDialog>, Mock.Of<ICustomCommandDialog>, Mock.Of<IAboutDialog>);

            Func<Task<DialogButton>> showMessageDialogAsnyc = () => dialogService.ShowMessageDialogAsync(title, content, buttons.ElementAt(0), buttons.ElementAt(1));

            await showMessageDialogAsnyc.Should().ThrowAsync<ArgumentNullException>().WithParameterName("content");
        }

        [Fact]
        public async Task ShowMessageDialogAsnyc_NoButtonsPassed_ThrowsArgumentException()
        {
            var title = _fixture.Create<string>();
            var content = _fixture.Create<string>();
            var dialogService = new DialogService(Mock.Of<IMessageDialog>, Mock.Of<ICreateKeyBindingDialog>, Mock.Of<IInputDialog>, Mock.Of<ISshConnectionInfoDialog>, Mock.Of<ICustomCommandDialog>, Mock.Of<IAboutDialog>);

            Func<Task<DialogButton>> showMessageDialogAsnyc = () => dialogService.ShowMessageDialogAsync(title, content);

            await showMessageDialogAsnyc.Should().ThrowAsync<ArgumentException>().WithParameterName("buttons");
        }

        [Fact]
        public void ShowMessageDialogAsnyc_Default_UsesMessageDialog()
        {
            var title = _fixture.Create<string>();
            var content = _fixture.Create<string>();
            var buttons = _fixture.CreateMany<DialogButton>(2);
            var messageDialog = new Mock<IMessageDialog>();
            var dialogService = new DialogService(() => messageDialog.Object, Mock.Of<ICreateKeyBindingDialog>, Mock.Of<IInputDialog>, Mock.Of<ISshConnectionInfoDialog>, Mock.Of<ICustomCommandDialog>, Mock.Of<IAboutDialog>);

            dialogService.ShowMessageDialogAsync(title, content, buttons.ElementAt(0), buttons.ElementAt(1));

            messageDialog.Verify(x => x.ShowAsync(), Times.Once);
        }
        
        [Fact]
        public void ShowInputDialogAsync_UsesIInputDialogSetTitle()
        {
            var title = "title";
            var inputDialog = new Mock<IInputDialog>();            
            var dialogService = new DialogService(Mock.Of<IMessageDialog>, Mock.Of<ICreateKeyBindingDialog>, () => inputDialog.Object, Mock.Of<ISshConnectionInfoDialog>, Mock.Of<ICustomCommandDialog>, Mock.Of<IAboutDialog>);

            dialogService.ShowInputDialogAsync(title);

            inputDialog.Verify(x => x.SetTitle(title), Times.Once);
            inputDialog.Verify(x => x.GetInput(), Times.Once);
        }
    }
}
