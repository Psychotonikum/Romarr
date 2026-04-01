using System;
using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Romarr.Common.Extensions;
using Romarr.Core.Test.Framework;
using Romarr.Core.Games;
using Romarr.Test.Common;

namespace Romarr.Core.Test.TvTests
{
    [TestFixture]
    public class RefreshRomServiceFixture : CoreTest<RefreshRomService>
    {
        private List<Rom> _insertedGameFiles;
        private List<Rom> _updatedGameFiles;
        private List<Rom> _deletedGameFiles;
        private Tuple<Game, List<Rom>> _gameOfThrones;

        [OneTimeSetUp]
        public void TestFixture()
        {
            var game = Builder<Game>.CreateNew()
                .With(s => s.IgdbId = 121361)
                .With(s => s.Title = "Test Game")
                .With(s => s.Runtime = 60)
                .With(s => s.Status = GameStatusType.Continuing)
                .With(s => s.GameType = GameTypes.Standard)
                .With(s => s.Platforms = new List<Platform>
                {
                    new Platform { PlatformNumber = 1, Monitored = true },
                    new Platform { PlatformNumber = 2, Monitored = true }
                })
                .Build();

            var roms = Builder<Rom>.CreateListOfSize(20)
                .All()
                .With(e => e.PlatformNumber = 1)
                .With(e => e.AirDateUtc = DateTime.UtcNow.AddDays(-30))
                .With(e => e.AirDate = DateTime.UtcNow.AddDays(-30).ToShortDateString())
                .Build()
                .ToList();

            for (var i = 0; i < roms.Count; i++)
            {
                roms[i].FileNumber = i + 1;
                roms[i].AbsoluteFileNumber = i + 1;
            }

            _gameOfThrones = new Tuple<Game, List<Rom>>(game, roms);
        }

        private List<Rom> GetRoms()
        {
            return _gameOfThrones.Item2.JsonClone();
        }

        private Game GetGame()
        {
            var game = _gameOfThrones.Item1.JsonClone();

            return game;
        }

        private Game GetAnimeSeries()
        {
            var game = Builder<Game>.CreateNew().Build();
            game.GameType = GameTypes.Standard;

            return game;
        }

        [SetUp]
        public void Setup()
        {
            _insertedGameFiles = new List<Rom>();
            _updatedGameFiles = new List<Rom>();
            _deletedGameFiles = new List<Rom>();

            Mocker.GetMock<IRomService>().Setup(c => c.InsertMany(It.IsAny<List<Rom>>()))
                .Callback<List<Rom>>(e => _insertedGameFiles = e);

            Mocker.GetMock<IRomService>().Setup(c => c.UpdateMany(It.IsAny<List<Rom>>()))
                .Callback<List<Rom>>(e => _updatedGameFiles = e);

            Mocker.GetMock<IRomService>().Setup(c => c.DeleteMany(It.IsAny<List<Rom>>()))
                .Callback<List<Rom>>(e => _deletedGameFiles = e);
        }

        [Test]
        public void should_create_all_when_no_existing_gameFiles()
        {
            Mocker.GetMock<IRomService>().Setup(c => c.GetGameFileBySeries(It.IsAny<int>()))
                .Returns(new List<Rom>());

            Subject.RefreshRomInfo(GetGame(), GetRoms());

            _insertedGameFiles.Should().HaveSameCount(GetRoms());
            _updatedGameFiles.Should().BeEmpty();
            _deletedGameFiles.Should().BeEmpty();

            ExceptionVerification.ExpectedWarns(1);
        }

        [Test]
        public void should_update_all_when_all_existing_gameFiles()
        {
            Mocker.GetMock<IRomService>().Setup(c => c.GetGameFileBySeries(It.IsAny<int>()))
                .Returns(GetRoms());

            Subject.RefreshRomInfo(GetGame(), GetRoms());

            _insertedGameFiles.Should().BeEmpty();
            _updatedGameFiles.Should().HaveSameCount(GetRoms());
            _deletedGameFiles.Should().BeEmpty();
        }

        [Test]
        public void should_delete_all_when_all_existing_gameFiles_are_gone_from_datasource()
        {
            Mocker.GetMock<IRomService>().Setup(c => c.GetGameFileBySeries(It.IsAny<int>()))
                .Returns(GetRoms());

            Subject.RefreshRomInfo(GetGame(), new List<Rom>());

            _insertedGameFiles.Should().BeEmpty();
            _updatedGameFiles.Should().BeEmpty();
            _deletedGameFiles.Should().HaveSameCount(GetRoms());
        }

        [Test]
        public void should_delete_duplicated_gameFiles_based_on_platform_gameFile_number()
        {
            var duplicateGameFiles = GetRoms().Skip(5).Take(2).ToList();

            Mocker.GetMock<IRomService>().Setup(c => c.GetGameFileBySeries(It.IsAny<int>()))
                .Returns(GetRoms().Union(duplicateGameFiles).ToList());

            Subject.RefreshRomInfo(GetGame(), GetRoms());

            _insertedGameFiles.Should().BeEmpty();
            _updatedGameFiles.Should().HaveSameCount(GetRoms());
            _deletedGameFiles.Should().HaveSameCount(duplicateGameFiles);
        }

        [Test]
        public void should_not_change_monitored_status_for_existing_gameFiles()
        {
            var game = GetGame();
            game.Platforms = new List<Platform>();
            game.Platforms.Add(new Platform { PlatformNumber = 1, Monitored = false });

            var roms = GetRoms();

            roms.ForEach(e => e.Monitored = true);

            Mocker.GetMock<IRomService>().Setup(c => c.GetGameFileBySeries(It.IsAny<int>()))
                .Returns(roms);

            Subject.RefreshRomInfo(game, GetRoms());

            _updatedGameFiles.Should().HaveSameCount(GetRoms());
            _updatedGameFiles.Should().OnlyContain(e => e.Monitored == true);
        }

        [Test]
        public void should_not_set_monitored_status_for_old_gameFiles_to_false_if_gameFiles_existed()
        {
            var game = GetGame();
            game.Platforms = new List<Platform>();
            game.Platforms.Add(new Platform { PlatformNumber = 1, Monitored = true });

            var roms = GetRoms().OrderBy(v => v.PlatformNumber).ThenBy(v => v.FileNumber).Take(5).ToList();

            roms[1].AirDateUtc = DateTime.UtcNow.AddDays(-15);
            roms[2].AirDateUtc = DateTime.UtcNow.AddDays(-10);
            roms[3].AirDateUtc = DateTime.UtcNow.AddDays(1);

            var existingRoms = roms.Skip(4).ToList();

            Mocker.GetMock<IRomService>().Setup(c => c.GetGameFileBySeries(It.IsAny<int>()))
                .Returns(existingRoms);

            Subject.RefreshRomInfo(game, roms);

            _insertedGameFiles = _insertedGameFiles.OrderBy(v => v.FileNumber).ToList();

            _insertedGameFiles.Should().HaveCount(4);
            _insertedGameFiles[0].Monitored.Should().Be(true);
            _insertedGameFiles[1].Monitored.Should().Be(true);
            _insertedGameFiles[2].Monitored.Should().Be(true);
            _insertedGameFiles[3].Monitored.Should().Be(true);

            ExceptionVerification.ExpectedWarns(1);
        }

        [Test]
        public void should_remove_duplicate_remote_gameFiles_before_processing()
        {
            Mocker.GetMock<IRomService>().Setup(c => c.GetGameFileBySeries(It.IsAny<int>()))
                .Returns(new List<Rom>());

            var roms = Builder<Rom>.CreateListOfSize(5)
                                           .TheFirst(2)
                                           .With(e => e.PlatformNumber = 1)
                                           .With(e => e.FileNumber = 1)
                                           .Build()
                                           .ToList();

            Subject.RefreshRomInfo(GetGame(), roms);

            _insertedGameFiles.Should().HaveCount(roms.Count - 1);
            _updatedGameFiles.Should().BeEmpty();
            _deletedGameFiles.Should().BeEmpty();
        }

        [Test]
        public void should_set_absolute_gameFile_number_for_anime()
        {
            var roms = Builder<Rom>.CreateListOfSize(3).Build().ToList();

            Mocker.GetMock<IRomService>().Setup(c => c.GetGameFileBySeries(It.IsAny<int>()))
                .Returns(new List<Rom>());

            Subject.RefreshRomInfo(GetAnimeSeries(), roms);

            _insertedGameFiles.All(e => e.AbsoluteFileNumber.HasValue).Should().BeTrue();
            _updatedGameFiles.Should().BeEmpty();
            _deletedGameFiles.Should().BeEmpty();
        }

        [Test]
        public void should_set_absolute_gameFile_number_even_if_not_previously_set_for_anime()
        {
            var roms = Builder<Rom>.CreateListOfSize(3).Build().ToList();

            var existingRoms = roms.JsonClone();
            existingRoms.ForEach(e => e.AbsoluteFileNumber = null);

            Mocker.GetMock<IRomService>().Setup(c => c.GetGameFileBySeries(It.IsAny<int>()))
                .Returns(existingRoms);

            Subject.RefreshRomInfo(GetAnimeSeries(), roms);

            _insertedGameFiles.Should().BeEmpty();
            _updatedGameFiles.All(e => e.AbsoluteFileNumber.HasValue).Should().BeTrue();
            _deletedGameFiles.Should().BeEmpty();
        }

        [Test]
        public void should_ignore_gameFiles_with_no_absolute_gameFile_in_distinct_by_absolute()
        {
            var roms = Builder<Rom>.CreateListOfSize(10)
                                           .Build()
                                           .ToList();

            roms[0].AbsoluteFileNumber = null;
            roms[1].AbsoluteFileNumber = null;
            roms[2].AbsoluteFileNumber = null;
            roms[3].AbsoluteFileNumber = null;
            roms[4].AbsoluteFileNumber = null;

            Mocker.GetMock<IRomService>().Setup(c => c.GetGameFileBySeries(It.IsAny<int>()))
                .Returns(new List<Rom>());

            Subject.RefreshRomInfo(GetAnimeSeries(), roms);

            _insertedGameFiles.Should().HaveCount(roms.Count);
        }

        [Test]
        public void should_override_empty_airdate_for_direct_to_dvd()
        {
            var game = GetGame();
            game.Status = GameStatusType.Ended;

            var roms = Builder<Rom>.CreateListOfSize(10)
                                           .All()
                                           .With(v => v.AirDateUtc = null)
                                           .BuildListOfNew();

            Mocker.GetMock<IRomService>().Setup(c => c.GetGameFileBySeries(It.IsAny<int>()))
                .Returns(new List<Rom>());

            List<Rom> updateGameFiles = null;
            Mocker.GetMock<IRomService>().Setup(c => c.InsertMany(It.IsAny<List<Rom>>()))
                .Callback<List<Rom>>(c => updateGameFiles = c);

            Subject.RefreshRomInfo(game, roms);

            updateGameFiles.Should().NotBeNull();
            updateGameFiles.Should().NotBeEmpty();
            updateGameFiles.All(v => v.AirDateUtc.HasValue).Should().BeTrue();
        }

        [Test]
        public void should_use_tba_for_gameFile_title_when_null()
        {
            Mocker.GetMock<IRomService>().Setup(c => c.GetGameFileBySeries(It.IsAny<int>()))
                .Returns(new List<Rom>());

            var roms = Builder<Rom>.CreateListOfSize(1)
                                           .All()
                                           .With(e => e.Title = null)
                                           .Build()
                                           .ToList();

            Subject.RefreshRomInfo(GetGame(), roms);

            _insertedGameFiles.First().Title.Should().Be("TBA");
        }

        [Test]
        public void should_update_air_date_when_multiple_gameFiles_air_on_the_same_day()
        {
            Mocker.GetMock<IRomService>().Setup(c => c.GetGameFileBySeries(It.IsAny<int>()))
                .Returns(new List<Rom>());

            var now = DateTime.UtcNow;
            var game = GetGame();

            var roms = Builder<Rom>.CreateListOfSize(2)
                                           .All()
                                           .With(e => e.PlatformNumber = 1)
                                           .With(e => e.AirDate = now.ToShortDateString())
                                           .With(e => e.AirDateUtc = now)
                                           .Build()
                                           .ToList();

            Subject.RefreshRomInfo(game, roms);

            _insertedGameFiles.First().AirDateUtc.Value.ToString("s").Should().Be(roms.First().AirDateUtc.Value.ToString("s"));
            _insertedGameFiles.Last().AirDateUtc.Value.ToString("s").Should().Be(roms.First().AirDateUtc.Value.AddMinutes(game.Runtime).ToString("s"));
        }

        [Test]
        public void should_not_update_air_date_when_more_than_three_gameFiles_air_on_the_same_day()
        {
            Mocker.GetMock<IRomService>().Setup(c => c.GetGameFileBySeries(It.IsAny<int>()))
                .Returns(new List<Rom>());

            var now = DateTime.UtcNow;
            var game = GetGame();

            var roms = Builder<Rom>.CreateListOfSize(4)
                                           .All()
                                           .With(e => e.PlatformNumber = 1)
                                           .With(e => e.AirDate = now.ToShortDateString())
                                           .With(e => e.AirDateUtc = now)
                                           .Build()
                                           .ToList();

            Subject.RefreshRomInfo(game, roms);

            _insertedGameFiles.Should().OnlyContain(e => e.AirDateUtc.Value.ToString("s") == roms.First().AirDateUtc.Value.ToString("s"));
        }

        [Test]
        public void should_match_anime_gameFiles_by_platform_and_gameFile_numbers()
        {
            var roms = Builder<Rom>.CreateListOfSize(2)
                .Build()
                .ToList();

            roms[0].AbsoluteFileNumber = null;
            roms[0].PlatformNumber.Should().NotBe(roms[1].PlatformNumber);
            roms[0].FileNumber.Should().NotBe(roms[1].FileNumber);

            var existingRom = new Rom
            {
                PlatformNumber = roms[0].PlatformNumber,
                FileNumber = roms[0].FileNumber,
                AbsoluteFileNumber = roms[1].AbsoluteFileNumber
            };

            Mocker.GetMock<IRomService>().Setup(c => c.GetGameFileBySeries(It.IsAny<int>()))
                .Returns(new List<Rom> { existingRom });

            Subject.RefreshRomInfo(GetAnimeSeries(), roms);

            _updatedGameFiles.First().PlatformNumber.Should().Be(roms[0].PlatformNumber);
            _updatedGameFiles.First().FileNumber.Should().Be(roms[0].FileNumber);
            _updatedGameFiles.First().AbsoluteFileNumber.Should().Be(roms[0].AbsoluteFileNumber);

            _insertedGameFiles.First().PlatformNumber.Should().Be(roms[1].PlatformNumber);
            _insertedGameFiles.First().FileNumber.Should().Be(roms[1].FileNumber);
            _insertedGameFiles.First().AbsoluteFileNumber.Should().Be(roms[1].AbsoluteFileNumber);
        }

        [Test]
        public void should_monitor_new_gameFile_if_platform_is_monitored()
        {
            var game = GetGame();
            game.Platforms = new List<Platform>();
            game.Platforms.Add(new Platform { PlatformNumber = 1, Monitored = true });

            var roms = Builder<Rom>.CreateListOfSize(2)
                .All()
                .With(e => e.PlatformNumber = 1)
                .Build()
                .ToList();

            var existingRom = new Rom
            {
                PlatformNumber = roms[0].PlatformNumber,
                FileNumber = roms[0].FileNumber,
                Monitored = true
            };

            Mocker.GetMock<IRomService>().Setup(c => c.GetGameFileBySeries(It.IsAny<int>()))
                .Returns(new List<Rom> { existingRom });

            Subject.RefreshRomInfo(game, roms);

            _updatedGameFiles.Should().HaveCount(1);
            _insertedGameFiles.Should().HaveCount(1);
            _insertedGameFiles.Should().OnlyContain(e => e.Monitored == true);
        }

        [Test]
        public void should_not_monitor_new_gameFile_if_platform_is_not_monitored()
        {
            var game = GetGame();
            game.Platforms = new List<Platform>();
            game.Platforms.Add(new Platform { PlatformNumber = 1, Monitored = false });

            var roms = Builder<Rom>.CreateListOfSize(2)
                .All()
                .With(e => e.PlatformNumber = 1)
                .Build()
                .ToList();

            var existingRom = new Rom
            {
                PlatformNumber = roms[0].PlatformNumber,
                FileNumber = roms[0].FileNumber,
                Monitored = true
            };

            Mocker.GetMock<IRomService>().Setup(c => c.GetGameFileBySeries(It.IsAny<int>()))
                .Returns(new List<Rom> { existingRom });

            Subject.RefreshRomInfo(game, roms);

            _updatedGameFiles.Should().HaveCount(1);
            _insertedGameFiles.Should().HaveCount(1);
            _insertedGameFiles.Should().OnlyContain(e => e.Monitored == false);
        }
    }
}
