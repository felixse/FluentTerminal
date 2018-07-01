using AutoFixture;
using FluentAssertions;
using FluentTerminal.App.Services.Implementation;
using FluentTerminal.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace FluentTerminal.App.Services.Test
{
    public class ITermThemeParserTests
    {
        private readonly Fixture _fixture;

        public ITermThemeParserTests()
        {
            _fixture = new Fixture();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void Parse_InvalidFileName_ThrowsArgumentNullException(string fileName)
        {
            var stream = typeof(ITermThemeParserTests).Assembly.GetManifestResourceStream("FluentTerminal.App.Services.Test.TestData.AdventureTime.itermcolors");
            var parser = new ITermThemeParser();

            Func<Task<TerminalTheme>> parse = () => parser.Parse(fileName, stream);

            parse.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("fileName");
        }

        [Fact]
        public void Parse_FileStreamIsNull_ThrowsArgumentNullException()
        {
            var fileName = _fixture.Create<string>();
            Stream stream = null;
            var parser = new ITermThemeParser();

            Func<Task<TerminalTheme>> parse = () => parser.Parse(fileName, stream);

            parse.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("fileContent");
        }

        [Fact]
        public void Parse_InvalidFileStream_ThrowsException()
        {
            var fileName = _fixture.Create<string>();
            var serialized = _fixture.Create<string>();
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(serialized));
            var parser = new ITermThemeParser();

            Func<Task<TerminalTheme>> parse = () => parser.Parse(fileName, stream);

            parse.Should().Throw<Exception>();
        }

        [Fact]
        public async Task Parse_ValidThemeFile_ParsesCorrectly()
        {
            var fileName = _fixture.Create<string>();
            var stream = typeof(ITermThemeParserTests).Assembly.GetManifestResourceStream("FluentTerminal.App.Services.Test.TestData.AdventureTime.itermcolors");
            var parser = new ITermThemeParser();

            var theme = await parser.Parse(fileName, stream);

            theme.Colors.Background.ToLower().Should().Be("#1f1d45");
            theme.Colors.Cursor.ToLower().Should().Be("#efbf38");
            theme.Colors.Foreground.ToLower().Should().Be("#f8dcc0");
            theme.Colors.Black.ToLower().Should().Be("#050404");
            theme.Colors.Red.ToLower().Should().Be("#bd0013");
            theme.Colors.Green.ToLower().Should().Be("#4ab118");
            theme.Colors.Yellow.ToLower().Should().Be("#e7741e");
            theme.Colors.Blue.ToLower().Should().Be("#0f4ac6");
            theme.Colors.Magenta.ToLower().Should().Be("#665993");
            theme.Colors.Cyan.ToLower().Should().Be("#70a598");
            theme.Colors.White.ToLower().Should().Be("#f8dcc0");
            theme.Colors.BrightBlack.ToLower().Should().Be("#4e7cbf");
            theme.Colors.BrightRed.ToLower().Should().Be("#fc5f5a");
            theme.Colors.BrightGreen.ToLower().Should().Be("#9eff6e");
            theme.Colors.BrightYellow.ToLower().Should().Be("#efc11a");
            theme.Colors.BrightBlue.ToLower().Should().Be("#1997c6");
            theme.Colors.BrightMagenta.ToLower().Should().Be("#9b5953");
            theme.Colors.BrightCyan.ToLower().Should().Be("#c8faf4");
            theme.Colors.BrightWhite.ToLower().Should().Be("#f6f5fb");
        }
    }
}
