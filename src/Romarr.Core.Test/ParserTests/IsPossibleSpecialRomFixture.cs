using FluentAssertions;
using NUnit.Framework;
using Romarr.Core.Parser.Model;

namespace Romarr.Core.Test.ParserTests
{
    [TestFixture]
    public class IsPossibleSpecialGameFileFixture
    {
        [Test]
        public void should_not_treat_files_without_a_series_title_as_a_special()
        {
            var parsedRomInfo = new ParsedRomInfo
                                    {
                                        RomNumbers = new[] { 7 },
                                        PlatformNumber = 1,
                                        GameTitle = ""
                                    };

            parsedRomInfo.IsPossibleSpecialGameFile.Should().BeFalse();
        }

        [Test]
        public void should_return_true_when_gameFile_numbers_is_empty()
        {
            var parsedRomInfo = new ParsedRomInfo
            {
                PlatformNumber = 1,
                GameTitle = ""
            };

            parsedRomInfo.IsPossibleSpecialGameFile.Should().BeTrue();
        }

        [TestCase("Title.the.Game.S02.Special-Inside.Chesters.Mill.HDTV.x264-BAJSKORV")]
        [TestCase("Title.the.Game.S02.Special-Inside.Chesters.Mill.720p.HDTV.x264-BAJSKORV")]
        [TestCase("Title.the.Game.S05.Special.HDTV.x264-2HD")]
        public void IsPossibleSpecialGameFile_should_be_true(string title)
        {
            Parser.Parser.ParseTitle(title).IsPossibleSpecialGameFile.Should().BeTrue();
        }

        [TestCase("Title.the.Game.S11E00.A.Christmas.Carol.Special.720p.HDTV-FieldOfView")]
        public void IsPossibleSpecialGameFile_should_be_true_if_e00_special(string title)
        {
            Parser.Parser.ParseTitle(title).IsPossibleSpecialGameFile.Should().BeTrue();
        }

        [TestCase("Big.Special.Show.S05.HDTV.x264-2HD")]
        public void IsPossibleSpecialGameFile_should_be_false_for_Special_in_title(string title)
        {
            Parser.Parser.ParseTitle(title).IsPossibleSpecialGameFile.Should().BeFalse();
        }
    }
}
