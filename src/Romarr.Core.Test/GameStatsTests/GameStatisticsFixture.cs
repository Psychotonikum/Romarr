using System;
using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using Romarr.Common.Extensions;
using Romarr.Core.Languages;
using Romarr.Core.MediaFiles;
using Romarr.Core.Qualities;
using Romarr.Core.GameStats;
using Romarr.Core.Test.Framework;
using Romarr.Core.Games;

namespace Romarr.Core.Test.GameStatsTests
{
    [TestFixture]
    public class GameStatisticsFixture : DbTest<GameStatisticsRepository, Game>
    {
        private Game _series;
        private Rom _gameFile;
        private RomFile _romFile;

        [SetUp]
        public void Setup()
        {
            _series = Builder<Game>.CreateNew()
                                        .With(s => s.Runtime = 30)
                                        .BuildNew();

            _series.Id = Db.Insert(_series).Id;

            _gameFile = Builder<Rom>.CreateNew()
                                          .With(e => e.RomFileId = 0)
                                          .With(e => e.Monitored = false)
                                          .With(e => e.GameId = _series.Id)
                                          .With(e => e.AirDateUtc = DateTime.Today.AddDays(5))
                                          .BuildNew();

            _romFile = Builder<RomFile>.CreateNew()
                                           .With(e => e.GameId = _series.Id)
                                           .With(e => e.Quality = new QualityModel(Quality.HDTV720p))
                                           .With(e => e.Languages = new List<Language> { Language.English })
                                           .BuildNew();
        }

        private void GivenGameFileWithFile()
        {
            _gameFile.RomFileId = 1;
        }

        private void GivenOldGameFile()
        {
            _gameFile.AirDateUtc = DateTime.Now.AddSeconds(-10);
        }

        private void GivenMonitoredGameFile()
        {
            _gameFile.Monitored = true;
        }

        private void GivenGameFile()
        {
            Db.Insert(_gameFile);
        }

        private void GivenRomFile()
        {
            Db.Insert(_romFile);
        }

        [Test]
        public void should_get_stats_for_series()
        {
            GivenMonitoredGameFile();
            GivenGameFile();

            var stats = Subject.GameStatistics();

            stats.Should().HaveCount(1);
            stats.First().NextAiring.Should().BeCloseTo(_gameFile.AirDateUtc.Value, TimeSpan.FromMilliseconds(1000));
            stats.First().PreviousAiring.Should().NotHaveValue();
        }

        [Test]
        public void should_not_have_next_airing_for_gameFile_with_file()
        {
            GivenGameFileWithFile();
            GivenGameFile();

            var stats = Subject.GameStatistics();

            stats.Should().HaveCount(1);
            stats.First().NextAiring.Should().NotHaveValue();
        }

        [Test]
        public void should_have_previous_airing_for_old_gameFile_without_file_monitored()
        {
            GivenMonitoredGameFile();
            GivenOldGameFile();
            GivenGameFile();

            var stats = Subject.GameStatistics();

            stats.Should().HaveCount(1);
            stats.First().NextAiring.Should().NotHaveValue();
            stats.First().PreviousAiring.Should().BeCloseTo(_gameFile.AirDateUtc.Value, TimeSpan.FromMilliseconds(1000));
        }

        [Test]
        public void should_not_have_previous_airing_for_old_gameFile_without_file_unmonitored()
        {
            GivenOldGameFile();
            GivenGameFile();

            var stats = Subject.GameStatistics();

            stats.Should().HaveCount(1);
            stats.First().NextAiring.Should().NotHaveValue();
            stats.First().PreviousAiring.Should().NotHaveValue();
        }

        [Test]
        public void should_not_include_unmonitored_gameFile_in_gameFile_count()
        {
            GivenGameFile();

            var stats = Subject.GameStatistics();

            stats.Should().HaveCount(1);
            stats.First().GameFileCount.Should().Be(0);
        }

        [Test]
        public void should_include_unmonitored_gameFile_with_file_in_gameFile_count()
        {
            GivenGameFileWithFile();
            GivenGameFile();

            var stats = Subject.GameStatistics();

            stats.Should().HaveCount(1);
            stats.First().GameFileCount.Should().Be(1);
        }

        [Test]
        public void should_have_size_on_disk_of_zero_when_no_gameFile_file()
        {
            GivenGameFile();

            var stats = Subject.GameStatistics();

            stats.Should().HaveCount(1);
            stats.First().SizeOnDisk.Should().Be(0);
        }

        [Test]
        public void should_have_size_on_disk_when_gameFile_file_exists()
        {
            GivenGameFileWithFile();
            GivenGameFile();
            GivenRomFile();

            var stats = Subject.GameStatistics();

            stats.Should().HaveCount(1);
            stats.First().SizeOnDisk.Should().Be(_romFile.Size);
        }

        [Test]
        public void should_not_duplicate_size_for_multi_gameFile_files()
        {
            GivenGameFileWithFile();
            GivenGameFile();
            GivenRomFile();

            var gameFile2 = _gameFile.JsonClone();

            gameFile2.Id = 0;
            gameFile2.FileNumber += 1;

            Db.Insert(gameFile2);

            var stats = Subject.GameStatistics();

            stats.Should().HaveCount(1);
            stats.First().SizeOnDisk.Should().Be(_romFile.Size);
        }
    }
}
