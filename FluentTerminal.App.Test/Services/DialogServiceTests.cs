using System;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using FluentTerminal.App.Services;
using FluentTerminal.App.Services.Implementation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace FluentTerminal.App.Test.Services
{
    [TestClass]
    public class DialogServiceTests
    {
        private Fixture _fixture;

        [TestInitialize]
        public void TestInitialize()
        {
            _fixture = new Fixture();
        }

        [TestMethod]
        public void ShowDialogAsync_TitleIsEmpty_ThrowsArgumentNullException()
        {
            var title = string.Empty;
            var content = _fixture.Create<string>();
            var buttons = _fixture.CreateMany<DialogButton>(2);
            var dialogService = new DialogService(Mock.Of<ISettingsService>());

            Func<Task<DialogButton>> invoke = () => dialogService.ShowMessageDialogAsnyc(title, content, buttons.ElementAt(0), buttons.ElementAt(1));

            invoke.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("title");
        }

        [TestMethod]
        public void ShowDialogAsync_ContentIsEmpty_ThrowsArgumentNullException()
        {
            var title = _fixture.Create<string>();
            var content = string.Empty;
            var buttons = _fixture.CreateMany<DialogButton>(2);
            var dialogService = new DialogService(Mock.Of<ISettingsService>());

            Func<Task<DialogButton>> invoke = () => dialogService.ShowMessageDialogAsnyc(title, content, buttons.ElementAt(0), buttons.ElementAt(1));

            invoke.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("content");
        }

        [TestMethod]
        public void ShowDialogAsync_NoButtonsPassed_ThrowsArgumentException()
        {
            var title = _fixture.Create<string>();
            var content = _fixture.Create<string>();
            var dialogService = new DialogService(Mock.Of<ISettingsService>());

            Func<Task<DialogButton>> invoke = () => dialogService.ShowMessageDialogAsnyc(title, content);

            invoke.Should().Throw<ArgumentException>().And.ParamName.Should().Be("buttons");
        }
    }
}
