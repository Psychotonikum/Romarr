using System;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Romarr.Core.Test.Framework;
using Romarr.Core.Games;

namespace Romarr.Core.Test.TvTests.RomServiceTests
{
    [TestFixture]
    public class ByAirDateFixture : CoreTest<RomService>
    {
        private const int SERIES_ID = 1;
        private const string AIR_DATE = "2014-04-02";

        private Rom CreateGameFile(int platformNumber, int romNumber)
        {
            var rom = Builder<Rom>.CreateNew()
                                          .With(e => e.GameId = 1)
                                          .With(e => e.PlatformNumber = platformNumber)
                                          .With(e => e.FileNumber = romNumber)
                                          .With(e => e.AirDate = AIR_DATE)
                                          .BuildNew();

            return rom;
        }

        private void GivenGameFiles(params Rom[] roms)
        {
            Mocker.GetMock<IRomRepository>()
                  .Setup(s => s.Find(It.IsAny<int>(), It.IsAny<string>()))
                  .Returns(roms.ToList());
        }

        [Test]
        public void should_throw_when_multiple_regular_gameFiles_are_found_and_not_part_provided()
        {
            GivenGameFiles(CreateGameFile(1, 1), CreateGameFile(2, 1));

            Assert.Throws<InvalidOperationException>(() => Subject.FindGameFile(SERIES_ID, AIR_DATE, null));
        }

        [Test]
        public void should_return_null_when_finds_no_gameFile()
        {
            GivenGameFiles();

            Subject.FindGameFile(SERIES_ID, AIR_DATE, null).Should().BeNull();
        }

        [Test]
        public void should_get_gameFile_when_single_gameFile_exists_for_air_date()
        {
            GivenGameFiles(CreateGameFile(1, 1));

            Subject.FindGameFile(SERIES_ID, AIR_DATE, null).Should().NotBeNull();
        }

        [Test]
        public void should_get_gameFile_when_regular_gameFile_and_special_share_the_same_air_date()
        {
            GivenGameFiles(CreateGameFile(1, 1), CreateGameFile(0, 1));

            Subject.FindGameFile(SERIES_ID, AIR_DATE, null).Should().NotBeNull();
        }

        [Test]
        public void should_get_special_when_its_the_only_gameFile_for_the_date_provided()
        {
            GivenGameFiles(CreateGameFile(0, 1));

            Subject.FindGameFile(SERIES_ID, AIR_DATE, null).Should().NotBeNull();
        }

        [Test]
        public void should_get_gameFile_when_two_regular_gameFiles_share_the_same_air_date_and_part_is_provided()
        {
            var gameFile1 = CreateGameFile(1, 1);
            var gameFile2 = CreateGameFile(1, 2);

            GivenGameFiles(gameFile1, gameFile2);

            Subject.FindGameFile(SERIES_ID, AIR_DATE, 1).Should().Be(gameFile1);
            Subject.FindGameFile(SERIES_ID, AIR_DATE, 2).Should().Be(gameFile2);
        }
    }
}
