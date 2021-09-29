using AutoFixture;
using FluentAssertions;
using FluentTerminal.App.Services.Implementation;
using FluentTerminal.Models;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace FluentTerminal.App.Services.Test
{
    public class FluentTerminalThemeParserTests
    {
        private readonly Fixture _fixture;

        public FluentTerminalThemeParserTests()
        {
            _fixture = new Fixture();
        }

        [Fact]
        public async Task Parse_ValidFileStream_ReturnsTheme()
        {
            var theme = _fixture.Create<TerminalTheme>();
            var serialized = JsonConvert.SerializeObject(theme);
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(serialized));
            var parser = new FluentTerminalThemeParser();

            var parsedTheme = await parser.Parse(theme.Name, stream);

            parsedTheme.Name.Should().Be(theme.Name);
            parsedTheme.Author.Should().Be(theme.Author);
            parsedTheme.PreInstalled.Should().Be(false);
            parsedTheme.Colors.Should().BeEquivalentTo(theme.Colors);
        }

        [Fact]
        public async Task Parse_InvalidFileStream_ThrowsException()
        {
            var fileName = _fixture.Create<string>();
            var serialized = _fixture.Create<string>();
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(serialized));
            var parser = new FluentTerminalThemeParser();

            Func<Task<TerminalTheme>> parse = () => parser.Parse(fileName, stream);

            await parse.Should().ThrowAsync<Exception>();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public async Task Parse_InvalidFileName_ThrowsArgumentNullException(string fileName)
        {
            var theme = _fixture.Create<TerminalTheme>();
            var serialized = JsonConvert.SerializeObject(theme);
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(serialized));
            var parser = new FluentTerminalThemeParser();

            Func<Task<TerminalTheme>> parse = () => parser.Parse(fileName, stream);

            await parse.Should().ThrowAsync<ArgumentNullException>().WithParameterName("fileName");
        }

        [Fact]
        public async Task Parse_FileStreamIsNull_ThrowsArgumentNullException()
        {
            var fileName = _fixture.Create<string>();
            Stream stream = null;
            var parser = new FluentTerminalThemeParser();

            Func<Task<TerminalTheme>> parse = () => parser.Parse(fileName, stream);

            await parse.Should().ThrowAsync<ArgumentNullException>().WithParameterName("fileContent");
        }
    }
}