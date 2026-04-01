using System;
using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Romarr.Core.DataAugmentation.Scene;
using Romarr.Core.IndexerSearch.Definitions;
using Romarr.Core.Languages;
using Romarr.Core.Parser;
using Romarr.Core.Parser.Model;
using Romarr.Core.Games;
using Romarr.Test.Common;

namespace Romarr.Core.Test.ParserTests.ParsingServiceTests
{
    [TestFixture]
    public class GetGameFilesFixture : TestBase<ParsingService>
    {
        private Game _series;
        private List<Rom> _gameFiles;
        private ParsedRomInfo _parsedRomInfo;
        private SingleGameFileSearchCriteria _singleGameFileSearchCriteria;

        [SetUp]
        public void Setup()
        {
            _series = Builder<Game>.CreateNew()
                .With(s => s.Title = "30 Stone")
                .With(s => s.CleanTitle = "stone")
                .Build();

            _gameFiles = Builder<Rom>.CreateListOfSize(1)
                                        .All()
                                        .With(e => e.AirDate = DateTime.Today.ToString(Rom.AIR_DATE_FORMAT))
                                        .Build()
                                        .ToList();

            _parsedRomInfo = new ParsedRomInfo
            {
                GameTitle = _series.Title,
                PlatformNumber = 1,
                RomNumbers = new[] { 1 },
                AbsoluteRomNumbers = Array.Empty<int>(),
                Languages = new List<Language> { Language.English }
            };

            _singleGameFileSearchCriteria = new SingleGameFileSearchCriteria
            {
                Game = _series,
                FileNumber = _gameFiles.First().FileNumber,
                PlatformNumber = _gameFiles.First().PlatformNumber,
                Roms = _gameFiles
            };

            Mocker.GetMock<IGameService>()
                  .Setup(s => s.FindByTitle(It.IsAny<string>()))
                  .Returns(_series);
        }

        private void GivenDailySeries()
        {
            _series.GameType = GameTypes.Standard;
        }

        private void GivenDailyParseResult()
        {
            _parsedRomInfo.AirDate = DateTime.Today.ToString(Rom.AIR_DATE_FORMAT);
        }

        private void GivenSceneNumberingSeries()
        {
            _series.UseSceneNumbering = true;
        }

        private void GivenAbsoluteNumberingSeries()
        {
            _parsedRomInfo.AbsoluteRomNumbers = new[] { 1 };
        }

        private void GivenFullPlatform()
        {
            _parsedRomInfo.FullPlatform = true;
            _parsedRomInfo.RomNumbers = Array.Empty<int>();
        }

        [Test]
        public void should_get_daily_gameFile_gameFile_when_search_criteria_is_null()
        {
            GivenDailySeries();
            GivenDailyParseResult();

            Subject.Map(_parsedRomInfo, _series.IgdbId, _series.MobyGamesId, _series.ImdbId);

            Mocker.GetMock<IRomService>()
                .Verify(v => v.FindGameFile(It.IsAny<int>(), It.IsAny<string>(), null), Times.Once());
        }

        [Test]
        public void should_use_search_criteria_gameFile_when_it_matches_daily()
        {
            GivenDailySeries();
            GivenDailyParseResult();

            Subject.Map(_parsedRomInfo, _series.IgdbId, _series.MobyGamesId, _series.ImdbId, _singleGameFileSearchCriteria);

            Mocker.GetMock<IRomService>()
                .Verify(v => v.FindGameFile(It.IsAny<int>(), It.IsAny<string>(), null), Times.Never());
        }

        [Test]
        public void should_fallback_to_daily_gameFile_lookup_when_search_criteria_gameFile_doesnt_match()
        {
            ExceptionVerification.IgnoreErrors();
            GivenDailySeries();
            _parsedRomInfo.AirDate = DateTime.Today.AddDays(-5).ToString(Rom.AIR_DATE_FORMAT);

            Subject.Map(_parsedRomInfo, _series.IgdbId, _series.MobyGamesId, _series.ImdbId, _singleGameFileSearchCriteria);

            Mocker.GetMock<IRomService>()
                .Verify(v => v.FindGameFile(It.IsAny<int>(), It.IsAny<string>(), null), Times.Once());
        }

        [Test]
        public void should_get_daily_gameFile_gameFile_should_lookup_including_daily_part()
        {
            GivenDailySeries();
            GivenDailyParseResult();
            _parsedRomInfo.DailyPart = 1;

            Subject.Map(_parsedRomInfo, _series.IgdbId, _series.MobyGamesId, _series.ImdbId);

            Mocker.GetMock<IRomService>()
                  .Verify(v => v.FindGameFile(It.IsAny<int>(), It.IsAny<string>(), 1), Times.Once());
        }

        [Test]
        public void should_use_search_criteria_gameFile_when_it_matches_absolute()
        {
            GivenAbsoluteNumberingSeries();

            Mocker.GetMock<IRomService>()
                  .Setup(s => s.FindGameFilesBySceneNumbering(It.IsAny<int>(), It.IsAny<int>()))
                  .Returns(new List<Rom>());

            Subject.Map(_parsedRomInfo, _series.IgdbId, _series.MobyGamesId, _series.ImdbId, _singleGameFileSearchCriteria);

            Mocker.GetMock<IRomService>()
                .Verify(v => v.FindGameFile(It.IsAny<int>(), It.IsAny<string>(), null), Times.Never());
        }

        [Test]
        public void should_use_scene_numbering_when_series_uses_scene_numbering()
        {
            GivenSceneNumberingSeries();

            Subject.Map(_parsedRomInfo, _series.IgdbId, _series.MobyGamesId, _series.ImdbId);

            Mocker.GetMock<IRomService>()
                .Verify(v => v.FindGameFilesBySceneNumbering(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()), Times.Once());
        }

        [Test]
        public void should_match_search_criteria_by_scene_numbering()
        {
            GivenSceneNumberingSeries();

            Subject.Map(_parsedRomInfo, _series.IgdbId, _series.MobyGamesId, _series.ImdbId, _singleGameFileSearchCriteria);

            Mocker.GetMock<IRomService>()
                .Verify(v => v.FindGameFilesBySceneNumbering(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()), Times.Never());
        }

        [Test]
        public void should_fallback_to_findGameFile_when_search_criteria_match_fails_for_scene_numbering()
        {
            GivenSceneNumberingSeries();
            _gameFiles.First().SceneFileNumber = 10;

            Subject.Map(_parsedRomInfo, _series.IgdbId, _series.MobyGamesId, _series.ImdbId, _singleGameFileSearchCriteria);

            Mocker.GetMock<IRomService>()
                .Verify(v => v.FindGameFilesBySceneNumbering(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()), Times.Once());
        }

        [Test]
        public void should_find_gameFile()
        {
            Subject.Map(_parsedRomInfo, _series.IgdbId, _series.MobyGamesId, _series.ImdbId);

            Mocker.GetMock<IRomService>()
                .Verify(v => v.FindGameFile(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()), Times.Once());
        }

        [Test]
        public void should_match_gameFile_with_search_criteria()
        {
            Subject.Map(_parsedRomInfo, _series.IgdbId, _series.MobyGamesId, _series.ImdbId, _singleGameFileSearchCriteria);

            Mocker.GetMock<IRomService>()
                .Verify(v => v.FindGameFile(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()), Times.Never());
        }

        [Test]
        public void should_fallback_to_findGameFile_when_search_criteria_match_fails()
        {
            _gameFiles.First().FileNumber = 10;

            Subject.Map(_parsedRomInfo, _series.IgdbId, _series.MobyGamesId, _series.ImdbId, _singleGameFileSearchCriteria);

            Mocker.GetMock<IRomService>()
                .Verify(v => v.FindGameFile(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()), Times.Once());
        }

        [Test]
        public void should_look_for_gameFile_in_platform_zero_if_absolute_special()
        {
            GivenAbsoluteNumberingSeries();

            _parsedRomInfo.Special = true;

            Subject.GetRoms(_parsedRomInfo, _series, true, null);

            Mocker.GetMock<IRomService>()
                  .Verify(v => v.FindGameFilesBySceneNumbering(It.IsAny<int>(), 0, It.IsAny<int>()), Times.Never());

            Mocker.GetMock<IRomService>()
                  .Verify(v => v.FindGameFile(It.IsAny<int>(), 0, It.IsAny<int>()), Times.Once());
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        public void should_use_scene_numbering_when_scene_platform_number_has_value(int platformNumber)
        {
            GivenAbsoluteNumberingSeries();

            Mocker.GetMock<ISceneMappingService>()
                  .Setup(s => s.GetScenePlatformNumber(_parsedRomInfo.GameTitle, It.IsAny<string>()))
                  .Returns(platformNumber);

            Mocker.GetMock<IRomService>()
                  .Setup(s => s.FindGameFilesBySceneNumbering(It.IsAny<int>(), platformNumber, It.IsAny<int>()))
                  .Returns(new List<Rom>());

            Subject.GetRoms(_parsedRomInfo, _series, true, null);

            Mocker.GetMock<IRomService>()
                  .Verify(v => v.FindGameFilesBySceneNumbering(It.IsAny<int>(), platformNumber, It.IsAny<int>()), Times.Once());

            Mocker.GetMock<IRomService>()
                  .Verify(v => v.FindGameFile(It.IsAny<int>(), platformNumber, It.IsAny<int>()), Times.Once());
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        public void should_find_gameFile_by_platform_and_scene_absolute_gameFile_number(int platformNumber)
        {
            GivenAbsoluteNumberingSeries();

            Mocker.GetMock<ISceneMappingService>()
                  .Setup(s => s.GetScenePlatformNumber(_parsedRomInfo.GameTitle, It.IsAny<string>()))
                  .Returns(platformNumber);

            Mocker.GetMock<IRomService>()
                  .Setup(s => s.FindGameFilesBySceneNumbering(It.IsAny<int>(), platformNumber, It.IsAny<int>()))
                  .Returns(new List<Rom> { _gameFiles.First() });

            Subject.GetRoms(_parsedRomInfo, _series, true, null);

            Mocker.GetMock<IRomService>()
                  .Verify(v => v.FindGameFilesBySceneNumbering(It.IsAny<int>(), platformNumber, It.IsAny<int>()), Times.Once());

            Mocker.GetMock<IRomService>()
                  .Verify(v => v.FindGameFile(It.IsAny<int>(), platformNumber, It.IsAny<int>()), Times.Never());
        }

        [TestCase(2)]
        [TestCase(20)]
        public void should_find_gameFile_by_parsed_platform_and_absolute_gameFile_number_when_platform_number_is_2_or_higher(int platformNumber)
        {
            GivenAbsoluteNumberingSeries();
            _parsedRomInfo.PlatformNumber = platformNumber;
            _parsedRomInfo.RomNumbers = Array.Empty<int>();

            Mocker.GetMock<IRomService>()
                  .Setup(s => s.FindGameFilesBySceneNumbering(It.IsAny<int>(), platformNumber, It.IsAny<int>()))
                  .Returns(new List<Rom> { _gameFiles.First() });

            Subject.GetRoms(_parsedRomInfo, _series, true, null);

            Mocker.GetMock<IRomService>()
                  .Verify(v => v.FindGameFilesBySceneNumbering(It.IsAny<int>(), platformNumber, It.IsAny<int>()), Times.Once());

            Mocker.GetMock<IRomService>()
                  .Verify(v => v.FindGameFile(It.IsAny<int>(), platformNumber, It.IsAny<int>()), Times.Never());
        }

        [TestCase(2)]
        [TestCase(20)]
        public void should_find_gameFile_by_parsed_platform_and_absolute_gameFile_number_when_platform_number_is_2_or_higher_and_scene_platform_number_lookup_failed(int platformNumber)
        {
            GivenAbsoluteNumberingSeries();
            _parsedRomInfo.PlatformNumber = platformNumber;
            _parsedRomInfo.RomNumbers = Array.Empty<int>();

            Mocker.GetMock<IRomService>()
                  .Setup(s => s.FindGameFilesBySceneNumbering(It.IsAny<int>(), platformNumber, It.IsAny<int>()))
                  .Returns(new List<Rom>());

            Mocker.GetMock<IRomService>()
                  .Setup(s => s.FindGameFile(It.IsAny<int>(), platformNumber, It.IsAny<int>()))
                  .Returns(_gameFiles.First());

            Subject.GetRoms(_parsedRomInfo, _series, true, null);

            Mocker.GetMock<IRomService>()
                  .Verify(v => v.FindGameFilesBySceneNumbering(It.IsAny<int>(), platformNumber, It.IsAny<int>()), Times.Once());

            Mocker.GetMock<IRomService>()
                  .Verify(v => v.FindGameFile(It.IsAny<int>(), platformNumber, It.IsAny<int>()), Times.Once());
        }

        [TestCase(2)]
        [TestCase(20)]
        public void should_not_find_gameFile_by_parsed_platform_and_absolute_gameFile_number_when_platform_number_is_2_or_higher_and_a_gameFile_number_was_parsed(int platformNumber)
        {
            GivenAbsoluteNumberingSeries();
            _parsedRomInfo.PlatformNumber = platformNumber;
            _parsedRomInfo.RomNumbers = new[] { 1 };

            Mocker.GetMock<IRomService>()
                  .Setup(s => s.FindGameFilesBySceneNumbering(It.IsAny<int>(), It.IsAny<int>()))
                  .Returns(new List<Rom> { _gameFiles.First() });

            Subject.GetRoms(_parsedRomInfo, _series, true, null);

            Mocker.GetMock<IRomService>()
                  .Verify(v => v.FindGameFilesBySceneNumbering(It.IsAny<int>(), platformNumber, It.IsAny<int>()), Times.Never());

            Mocker.GetMock<IRomService>()
                  .Verify(v => v.FindGameFile(It.IsAny<int>(), platformNumber, It.IsAny<int>()), Times.Never());
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        public void should_return_gameFiles_when_scene_absolute_gameFile_number_returns_multiple_results(int platformNumber)
        {
            GivenAbsoluteNumberingSeries();

            Mocker.GetMock<ISceneMappingService>()
                  .Setup(s => s.GetScenePlatformNumber(_parsedRomInfo.GameTitle, It.IsAny<string>()))
                  .Returns(platformNumber);

            Mocker.GetMock<IRomService>()
                  .Setup(s => s.FindGameFilesBySceneNumbering(It.IsAny<int>(), platformNumber, It.IsAny<int>()))
                  .Returns(Builder<Rom>.CreateListOfSize(5).Build().ToList());

            var result = Subject.GetRoms(_parsedRomInfo, _series, true, null);

            result.Should().HaveCount(5);

            Mocker.GetMock<IRomService>()
                  .Verify(v => v.FindGameFilesBySceneNumbering(It.IsAny<int>(), platformNumber, It.IsAny<int>()), Times.Once());

            Mocker.GetMock<IRomService>()
                  .Verify(v => v.FindGameFile(It.IsAny<int>(), platformNumber, It.IsAny<int>()), Times.Never());
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        public void should_find_gameFile_by_platform_and_absolute_gameFile_number_when_scene_absolute_gameFile_number_returns_no_results(int platformNumber)
        {
            GivenAbsoluteNumberingSeries();

            Mocker.GetMock<ISceneMappingService>()
                  .Setup(s => s.GetScenePlatformNumber(_parsedRomInfo.GameTitle, It.IsAny<string>()))
                  .Returns(platformNumber);

            Mocker.GetMock<IRomService>()
                  .Setup(s => s.FindGameFilesBySceneNumbering(It.IsAny<int>(), platformNumber, It.IsAny<int>()))
                  .Returns(new List<Rom>());

            Subject.GetRoms(_parsedRomInfo, _series, true, null);

            Mocker.GetMock<IRomService>()
                  .Verify(v => v.FindGameFilesBySceneNumbering(It.IsAny<int>(), platformNumber, It.IsAny<int>()), Times.Once());

            Mocker.GetMock<IRomService>()
                  .Verify(v => v.FindGameFile(It.IsAny<int>(), platformNumber, It.IsAny<int>()), Times.Once());
        }

        [Test]
        public void should_use_igdb_platform_number_when_available_and_a_scene_source()
        {
            const int igdbPlatformNumber = 5;

            Mocker.GetMock<ISceneMappingService>()
                  .Setup(v => v.FindSceneMapping(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
                  .Returns<string, string, int>((s, r, sn) => new SceneMapping { ScenePlatformNumber = 1, PlatformNumber = igdbPlatformNumber });

            Subject.GetRoms(_parsedRomInfo, _series, true, null);

            Mocker.GetMock<IRomService>()
                .Verify(v => v.FindGameFile(_series.Id, _parsedRomInfo.PlatformNumber, _parsedRomInfo.RomNumbers.First()), Times.Never());

            Mocker.GetMock<IRomService>()
                .Verify(v => v.FindGameFile(_series.Id, igdbPlatformNumber, _parsedRomInfo.RomNumbers.First()), Times.Once());
        }

        [Test]
        public void should_not_use_igdb_platform_number_when_available_for_a_different_platform_and_a_scene_source()
        {
            const int igdbPlatformNumber = 5;

            Mocker.GetMock<ISceneMappingService>()
                  .Setup(v => v.FindSceneMapping(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
                  .Returns<string, string, int>((s, r, sn) => new SceneMapping { ScenePlatformNumber = 101, PlatformNumber = igdbPlatformNumber });

            Subject.GetRoms(_parsedRomInfo, _series, true, null);

            Mocker.GetMock<IRomService>()
                .Verify(v => v.FindGameFile(_series.Id, igdbPlatformNumber, _parsedRomInfo.RomNumbers.First()), Times.Never());

            Mocker.GetMock<IRomService>()
                .Verify(v => v.FindGameFile(_series.Id, _parsedRomInfo.PlatformNumber, _parsedRomInfo.RomNumbers.First()), Times.Once());
        }

        [Test]
        public void should_not_use_igdb_platform_when_not_a_scene_source()
        {
            const int igdbPlatformNumber = 5;

            Subject.GetRoms(_parsedRomInfo, _series, false, null);

            Mocker.GetMock<IRomService>()
                .Verify(v => v.FindGameFile(_series.Id, igdbPlatformNumber, _parsedRomInfo.RomNumbers.First()), Times.Never());

            Mocker.GetMock<IRomService>()
                .Verify(v => v.FindGameFile(_series.Id, _parsedRomInfo.PlatformNumber, _parsedRomInfo.RomNumbers.First()), Times.Once());
        }

        [Test]
        public void should_not_use_igdb_platform_when_igdb_platform_number_is_less_than_zero()
        {
            const int igdbPlatformNumber = -1;

            Mocker.GetMock<ISceneMappingService>()
                  .Setup(s => s.FindSceneMapping(_parsedRomInfo.GameTitle, It.IsAny<string>(), It.IsAny<int>()))
                  .Returns(new SceneMapping { PlatformNumber = igdbPlatformNumber, ScenePlatformNumber = _parsedRomInfo.PlatformNumber });

            Subject.GetRoms(_parsedRomInfo, _series, true, null);

            Mocker.GetMock<IRomService>()
                .Verify(v => v.FindGameFile(_series.Id, igdbPlatformNumber, _parsedRomInfo.RomNumbers.First()), Times.Never());

            Mocker.GetMock<IRomService>()
                .Verify(v => v.FindGameFile(_series.Id, _parsedRomInfo.PlatformNumber, _parsedRomInfo.RomNumbers.First()), Times.Once());
        }

        [Test]
        public void should_lookup_full_platform_by_platform_number_if_series_does_not_use_scene_numbering()
        {
            GivenFullPlatform();

            Mocker.GetMock<IRomService>()
                .Setup(s => s.GetRomsByPlatform(_series.Id, _parsedRomInfo.PlatformNumber))
                .Returns(_gameFiles);

            Subject.GetRoms(_parsedRomInfo, _series, true, null);

            Mocker.GetMock<IRomService>()
                .Verify(v => v.GetRomsByPlatform(It.IsAny<int>(), It.IsAny<int>()), Times.Once);

            Mocker.GetMock<IRomService>()
                .Verify(v => v.GetRomsByScenePlatform(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        }

        [Test]
        public void should_lookup_full_platform_by_scene_platform_number_if_series_uses_scene_numbering()
        {
            GivenSceneNumberingSeries();
            GivenFullPlatform();

            Mocker.GetMock<IRomService>()
                .Setup(s => s.GetRomsByScenePlatform(_series.Id, _parsedRomInfo.PlatformNumber))
                .Returns(_gameFiles);

            Subject.GetRoms(_parsedRomInfo, _series, true, null);

            Mocker.GetMock<IRomService>()
                .Verify(v => v.GetRomsByPlatform(It.IsAny<int>(), It.IsAny<int>()), Times.Never);

            Mocker.GetMock<IRomService>()
                .Verify(v => v.GetRomsByScenePlatform(It.IsAny<int>(), It.IsAny<int>()), Times.Once);
        }

        [Test]
        public void should_fallback_to_lookup_full_platform_by_platform_number_if_series_uses_scene_numbering_and_no_epsiodes_are_found_by_scene_platform_number()
        {
            GivenSceneNumberingSeries();
            GivenFullPlatform();

            Mocker.GetMock<IRomService>()
                .Setup(s => s.GetRomsByScenePlatform(_series.Id, _parsedRomInfo.PlatformNumber))
                .Returns(new List<Rom>());

            Mocker.GetMock<IRomService>()
                .Setup(s => s.GetRomsByPlatform(_series.Id, _parsedRomInfo.PlatformNumber))
                .Returns(_gameFiles);

            Subject.GetRoms(_parsedRomInfo, _series, true, null);

            Mocker.GetMock<IRomService>()
                .Verify(v => v.GetRomsByPlatform(It.IsAny<int>(), It.IsAny<int>()), Times.Once);

            Mocker.GetMock<IRomService>()
                .Verify(v => v.GetRomsByScenePlatform(It.IsAny<int>(), It.IsAny<int>()), Times.Once);
        }

        [Test]
        public void should_use_platform_zero_when_looking_up_is_partial_special_gameFile_found_by_title()
        {
            _series.UseSceneNumbering = false;
            _parsedRomInfo.PlatformNumber = 1;
            _parsedRomInfo.RomNumbers = new int[] { 0 };
            _parsedRomInfo.ReleaseTitle = "Game.Title.S01E00.My.Special.Rom.1080p.AMZN.WEB-DL.DDP5.1.H264-TEPES";

            Mocker.GetMock<IRomService>()
                  .Setup(s => s.FindGameFileByTitle(_series.IgdbId, 0, _parsedRomInfo.ReleaseTitle))
                  .Returns(
                      Builder<Rom>.CreateNew()
                                      .With(e => e.PlatformNumber = 0)
                                      .With(e => e.FileNumber = 1)
                                      .Build());

            Subject.Map(_parsedRomInfo, _series.IgdbId, _series.MobyGamesId, _series.ImdbId);

            Mocker.GetMock<IRomService>()
                  .Verify(v => v.FindGameFile(_series.IgdbId, 0, 1), Times.Once());
        }

        [Test]
        public void should_use_original_parse_result_when_special_gameFile_lookup_by_title_fails()
        {
            _series.UseSceneNumbering = false;
            _parsedRomInfo.PlatformNumber = 1;
            _parsedRomInfo.RomNumbers = new int[] { 0 };
            _parsedRomInfo.ReleaseTitle = "Game.Title.S01E00.My.Special.Rom.1080p.AMZN.WEB-DL.DDP5.1.H264-TEPES";

            Mocker.GetMock<IRomService>()
                  .Setup(s => s.FindGameFileByTitle(_series.IgdbId, 0, _parsedRomInfo.ReleaseTitle))
                  .Returns((Rom)null);

            Subject.Map(_parsedRomInfo, _series.IgdbId, _series.MobyGamesId, _series.ImdbId);

            Mocker.GetMock<IRomService>()
                  .Verify(v => v.FindGameFile(_series.IgdbId, _parsedRomInfo.PlatformNumber, _parsedRomInfo.RomNumbers.First()), Times.Once());
        }
    }
}
