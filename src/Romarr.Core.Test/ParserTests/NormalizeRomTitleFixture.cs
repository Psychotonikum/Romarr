using FluentAssertions;
using NUnit.Framework;
using Romarr.Core.Test.Framework;

namespace Romarr.Core.Test.ParserTests
{
    [TestFixture]
    public class NormalizeRomTitleFixture : CoreTest
    {
        [TestCase("Rom Title", "rom title")]
        [TestCase("A.B,C;", "a b c")]
        [TestCase("Rom  Title", "rom title")]
        [TestCase("French Title (1)", "french title")]
        [TestCase("Game.Title.S01.Special.Rom.Title.720p.HDTV.x264-Romarr", "rom title")]
        [TestCase("Game.Title.S01E00.Rom.Title.720p.HDTV.x264-Romarr", "rom title")]
        public void should_normalize_gameFile_title(string input, string expected)
        {
            var result = Parser.Parser.NormalizeRomTitle(input);

            result.Should().Be(expected);
        }
    }
}
