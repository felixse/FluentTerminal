using AutoFixture;
using FluentAssertions;
using FluentTerminal.App.Services.Implementation;
using Moq;
using System.Linq;
using Xunit;

namespace FluentTerminal.App.Services.Test
{
    public class ThemeParserFactoryTests
    {
        private readonly Fixture _fixture;

        public ThemeParserFactoryTests()
        {
            _fixture = new Fixture();
        }

        [Fact]
        public void GetParser_SupportedFileType_ReturnsParser()
        {
            var supportedFileType = _fixture.Create<string>();
            var parser = new Mock<IThemeParser>();
            parser.Setup(x => x.SupportedFileTypes).Returns(new string[] { supportedFileType });
            var themeParserFactory = new ThemeParserFactory(new IThemeParser[] { parser.Object });

            var result = themeParserFactory.GetParser(supportedFileType);

            parser.Should().Be(parser);
        }

        [Fact]
        public void GetParser_UnsupportedFileType_ReturnsNull()
        {
            var unSupportedFileType = _fixture.Create<string>();
            var supportedFileType = _fixture.Create<string>();
            var parser = new Mock<IThemeParser>();
            parser.Setup(x => x.SupportedFileTypes).Returns(new string[] { supportedFileType });
            var themeParserFactory = new ThemeParserFactory(new IThemeParser[] { parser.Object });

            var result = themeParserFactory.GetParser(unSupportedFileType);

            result.Should().BeNull();
        }
    }
}
