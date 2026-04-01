using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Romarr.Core.DataAugmentation.Scene;
using Romarr.Core.DecisionEngine;
using Romarr.Core.Indexers;
using Romarr.Core.IndexerSearch;
using Romarr.Core.IndexerSearch.Definitions;
using Romarr.Core.Test.Framework;
using Romarr.Core.Games;

namespace Romarr.Core.Test.IndexerSearchTests
{
    public class ReleaseSearchServiceFixture : CoreTest<ReleaseSearchService>
    {
        private Mock<IIndexer> _mockIndexer;
        private Game _xemGame;
        private List<Rom> _xemRoms;

        [SetUp]
        public void SetUp()
        {
            _mockIndexer = Mocker.GetMock<IIndexer>();
            _mockIndexer.SetupGet(s => s.Definition).Returns(new IndexerDefinition { Id = 1 });
            _mockIndexer.SetupGet(s => s.SupportsSearch).Returns(true);

            Mocker.GetMock<IIndexerFactory>()
                  .Setup(s => s.AutomaticSearchEnabled(true))
                  .Returns(new List<IIndexer> { _mockIndexer.Object });

            Mocker.GetMock<IMakeDownloadDecision>()
                .Setup(s => s.GetSearchDecision(It.IsAny<List<Parser.Model.ReleaseInfo>>(), It.IsAny<SearchCriteriaBase>()))
                .Returns(new List<DownloadDecision>());

            _xemGame = Builder<Game>.CreateNew()
                .With(v => v.UseSceneNumbering = true)
                .With(v => v.Monitored = true)
                .Build();

            _xemRoms = new List<Rom>();

            Mocker.GetMock<IGameService>()
                .Setup(v => v.GetGame(_xemGame.Id))
                .Returns(_xemGame);

            Mocker.GetMock<IRomService>()
                .Setup(v => v.GetRomsByPlatform(_xemGame.Id, It.IsAny<int>()))
                .Returns<int, int>((i, j) => _xemRoms.Where(d => d.PlatformNumber == j).ToList());

            Mocker.GetMock<ISceneMappingService>()
                  .Setup(s => s.FindByIgdbId(It.IsAny<int>()))
                  .Returns(new List<SceneMapping>());

            Mocker.GetMock<ISceneMappingService>()
                  .Setup(s => s.GetSceneNames(It.IsAny<int>(), It.IsAny<List<int>>(), It.IsAny<List<int>>()))
                  .Returns(new List<string>());
        }

        private void WithRom(int platformNumber, int romNumber, int? scenePlatformNumber, int? sceneRomNumber, string airDate = null)
        {
            var rom = Builder<Rom>.CreateNew()
                .With(v => v.GameId == _xemGame.Id)
                .With(v => v.Game == _xemGame)
                .With(v => v.PlatformNumber, platformNumber)
                .With(v => v.FileNumber, romNumber)
                .With(v => v.ScenePlatformNumber, scenePlatformNumber)
                .With(v => v.SceneFileNumber, sceneRomNumber)
                .With(v => v.AirDate = airDate ?? $"{2000 + platformNumber}-{(romNumber % 12) + 1:00}-05")
                .With(v => v.AirDateUtc = DateTime.ParseExact(v.AirDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal).ToUniversalTime())
                .With(v => v.Monitored = true)
                .Build();

            _xemRoms.Add(rom);
        }

        private void WithRoms()
        {
            // Platform 1 maps to Scene Platform 2 (one-to-one)
            WithRom(1, 12, 2, 3);
            WithRom(1, 13, 2, 4);

            // Platform 2 maps to Scene Platform 3 & 4 (one-to-one)
            WithRom(2, 1, 3, 11);
            WithRom(2, 2, 3, 12);
            WithRom(2, 3, 4, 11);
            WithRom(2, 4, 4, 12);

            // Platform 3 maps to Scene Platform 5 (partial)
            // Platform 4 maps to Scene Platform 5 & 6 (partial)
            WithRom(3, 1, 5, 11);
            WithRom(3, 2, 5, 12);
            WithRom(4, 1, 5, 13);
            WithRom(4, 2, 5, 14);
            WithRom(4, 3, 6, 11);
            WithRom(5, 1, 6, 12);

            // Platform 7+ maps normally, so no mapping specified.
            WithRom(7, 1, null, null);
            WithRom(7, 2, null, null);
        }

        private List<SearchCriteriaBase> WatchForSearchCriteria()
        {
            var result = new List<SearchCriteriaBase>();

            _mockIndexer.Setup(v => v.Fetch(It.IsAny<SingleGameFileSearchCriteria>()))
                .Callback<SingleGameFileSearchCriteria>(s => result.Add(s))
                .Returns(Task.FromResult<IList<Parser.Model.ReleaseInfo>>(new List<Parser.Model.ReleaseInfo>()));

            _mockIndexer.Setup(v => v.Fetch(It.IsAny<PlatformSearchCriteria>()))
                .Callback<PlatformSearchCriteria>(s => result.Add(s))
                .Returns(Task.FromResult<IList<Parser.Model.ReleaseInfo>>(new List<Parser.Model.ReleaseInfo>()));

            _mockIndexer.Setup(v => v.Fetch(It.IsAny<PlatformSearchCriteria>()))
                .Callback<PlatformSearchCriteria>(s => result.Add(s))
                .Returns(Task.FromResult<IList<Parser.Model.ReleaseInfo>>(new List<Parser.Model.ReleaseInfo>()));

            _mockIndexer.Setup(v => v.Fetch(It.IsAny<PlatformSearchCriteria>()))
                .Callback<PlatformSearchCriteria>(s => result.Add(s))
                .Returns(Task.FromResult<IList<Parser.Model.ReleaseInfo>>(new List<Parser.Model.ReleaseInfo>()));

            _mockIndexer.Setup(v => v.Fetch(It.IsAny<SingleGameFileSearchCriteria>()))
                .Callback<SingleGameFileSearchCriteria>(s => result.Add(s))
                .Returns(Task.FromResult<IList<Parser.Model.ReleaseInfo>>(new List<Parser.Model.ReleaseInfo>()));

            _mockIndexer.Setup(v => v.Fetch(It.IsAny<PlatformSearchCriteria>()))
                .Callback<PlatformSearchCriteria>(s => result.Add(s))
                .Returns(Task.FromResult<IList<Parser.Model.ReleaseInfo>>(new List<Parser.Model.ReleaseInfo>()));

            _mockIndexer.Setup(v => v.Fetch(It.IsAny<SpecialGameFileSearchCriteria>()))
                .Callback<SpecialGameFileSearchCriteria>(s => result.Add(s))
                .Returns(Task.FromResult<IList<Parser.Model.ReleaseInfo>>(new List<Parser.Model.ReleaseInfo>()));

            return result;
        }

        [Test]
        public async Task Tags_IndexerTags_SeriesNoTags_IndexerNotIncluded()
        {
            _mockIndexer.SetupGet(s => s.Definition).Returns(new IndexerDefinition
            {
                Id = 1,
                Tags = new HashSet<int> { 3 }
            });

            WithRoms();

            var allCriteria = WatchForSearchCriteria();

            await Subject.RomSearch(_xemRoms.First(), true, false);

            var criteria = allCriteria.OfType<SingleGameFileSearchCriteria>().ToList();

            criteria.Count.Should().Be(0);
        }

        [Test]
        public async Task Tags_IndexerNoTags_GameTags_IndexerIncluded()
        {
            _mockIndexer.SetupGet(s => s.Definition).Returns(new IndexerDefinition
            {
                Id = 1
            });

            _xemGame = Builder<Game>.CreateNew()
                .With(v => v.UseSceneNumbering = true)
                .With(v => v.Monitored = true)
                .With(v => v.Tags = new HashSet<int> { 3 })
                .Build();

            Mocker.GetMock<IGameService>()
                .Setup(v => v.GetGame(_xemGame.Id))
                .Returns(_xemGame);

            WithRoms();

            var allCriteria = WatchForSearchCriteria();

            await Subject.RomSearch(_xemRoms.First(), true, false);

            var criteria = allCriteria.OfType<SingleGameFileSearchCriteria>().ToList();

            criteria.Count.Should().Be(1);
        }

        [Test]
        public async Task Tags_IndexerAndGameTagsMatch_IndexerIncluded()
        {
            _mockIndexer.SetupGet(s => s.Definition).Returns(new IndexerDefinition
            {
                Id = 1,
                Tags = new HashSet<int> { 1, 2, 3 }
            });

            _xemGame = Builder<Game>.CreateNew()
                .With(v => v.UseSceneNumbering = true)
                .With(v => v.Monitored = true)
                .With(v => v.Tags = new HashSet<int> { 3, 4, 5 })
                .Build();

            Mocker.GetMock<IGameService>()
                .Setup(v => v.GetGame(_xemGame.Id))
                .Returns(_xemGame);

            WithRoms();

            var allCriteria = WatchForSearchCriteria();

            await Subject.RomSearch(_xemRoms.First(), true, false);

            var criteria = allCriteria.OfType<SingleGameFileSearchCriteria>().ToList();

            criteria.Count.Should().Be(1);
        }

        [Test]
        public async Task Tags_IndexerAndGameTagsMismatch_IndexerNotIncluded()
        {
            _mockIndexer.SetupGet(s => s.Definition).Returns(new IndexerDefinition
            {
                Id = 1,
                Tags = new HashSet<int> { 1, 2, 3 }
            });

            _xemGame = Builder<Game>.CreateNew()
                .With(v => v.UseSceneNumbering = true)
                .With(v => v.Monitored = true)
                .With(v => v.Tags = new HashSet<int> { 4, 5, 6 })
                .Build();

            Mocker.GetMock<IGameService>()
                .Setup(v => v.GetGame(_xemGame.Id))
                .Returns(_xemGame);

            WithRoms();

            var allCriteria = WatchForSearchCriteria();

            await Subject.RomSearch(_xemRoms.First(), true, false);

            var criteria = allCriteria.OfType<SingleGameFileSearchCriteria>().ToList();

            criteria.Count.Should().Be(0);
        }

        [Test]
        public async Task scene_romsearch()
        {
            WithRoms();

            var allCriteria = WatchForSearchCriteria();

            await Subject.RomSearch(_xemRoms.First(), true, false);

            var criteria = allCriteria.OfType<SingleGameFileSearchCriteria>().ToList();

            criteria.Count.Should().Be(1);
            criteria[0].PlatformNumber.Should().Be(2);
            criteria[0].FileNumber.Should().Be(3);
        }

        [Test]
        public async Task scene_platformsearch()
        {
            WithRoms();

            var allCriteria = WatchForSearchCriteria();

            await Subject.PlatformSearch(_xemGame.Id, 1, false, false, true, false);

            var criteria = allCriteria.OfType<PlatformSearchCriteria>().ToList();

            criteria.Count.Should().Be(1);
            criteria[0].PlatformNumber.Should().Be(2);
        }

        [Test]
        public async Task scene_platformsearch_should_search_multiple_platforms()
        {
            WithRoms();

            var allCriteria = WatchForSearchCriteria();

            await Subject.PlatformSearch(_xemGame.Id, 2, false, false, true, false);

            var criteria = allCriteria.OfType<PlatformSearchCriteria>().ToList();

            criteria.Count.Should().Be(2);
            criteria[0].PlatformNumber.Should().Be(3);
            criteria[1].PlatformNumber.Should().Be(4);
        }

        [Test]
        public async Task scene_platformsearch_should_search_single_gameFile_if_possible()
        {
            WithRoms();

            var allCriteria = WatchForSearchCriteria();

            await Subject.PlatformSearch(_xemGame.Id, 4, false, false, true, false);

            var criteria1 = allCriteria.OfType<PlatformSearchCriteria>().ToList();
            var criteria2 = allCriteria.OfType<SingleGameFileSearchCriteria>().ToList();

            criteria1.Count.Should().Be(1);
            criteria1[0].PlatformNumber.Should().Be(5);

            criteria2.Count.Should().Be(1);
            criteria2[0].PlatformNumber.Should().Be(6);
            criteria2[0].FileNumber.Should().Be(11);
        }

        [Test]
        public async Task scene_platformsearch_should_use_platformnumber_if_no_scene_number_is_available()
        {
            WithRoms();

            var allCriteria = WatchForSearchCriteria();

            await Subject.PlatformSearch(_xemGame.Id, 7, false, false, true, false);

            var criteria = allCriteria.OfType<PlatformSearchCriteria>().ToList();

            criteria.Count.Should().Be(1);
            criteria[0].PlatformNumber.Should().Be(7);
        }

        [Test]
        public async Task scene_platformsearch_should_skip_search_if_no_gameFiles_after_filtering()
        {
            WithRoms();
            _xemRoms.ForEach(e => e.RomFileId = 1);

            var allCriteria = WatchForSearchCriteria();

            await Subject.PlatformSearch(_xemGame.Id, 1, true, false, true, false);

            var criteria = allCriteria.OfType<PlatformSearchCriteria>().ToList();

            criteria.Count.Should().Be(0);
        }

        [Test]
        public async Task platform_search_for_standard_should_not_search_for_gameFiles_with_files()
        {
            WithRoms();
            _xemGame.GameType = GameTypes.Standard;
            _xemRoms.ForEach(e => e.RomFileId = 1);

            var platformNumber = 1;
            var allCriteria = WatchForSearchCriteria();

            await Subject.PlatformSearch(_xemGame.Id, platformNumber, true, false, true, false);

            var criteria = allCriteria.OfType<SingleGameFileSearchCriteria>().ToList();

            criteria.Count.Should().Be(0);
        }

        [Test]
        public async Task platform_search_for_standard_should_search_for_each_monitored_platform()
        {
            WithRoms();
            _xemGame.GameType = GameTypes.Standard;
            _xemRoms.ForEach(e => e.RomFileId = 0);

            var platformNumber = 1;
            var allCriteria = WatchForSearchCriteria();

            await Subject.PlatformSearch(_xemGame.Id, platformNumber, true, false, true, false);

            var criteria = allCriteria.OfType<PlatformSearchCriteria>().ToList();

            var gameFilesForPlatform1 = _xemRoms.Where(e => e.PlatformNumber == platformNumber);
            criteria.Count.Should().Be(gameFilesForPlatform1.Select(e => e.PlatformNumber).Distinct().Count());
        }

        [Test]
        public async Task platform_search_for_standard_should_not_search_for_platform_with_files()
        {
            WithRoms();
            _xemGame.GameType = GameTypes.Standard;
            _xemRoms.ForEach(e => e.RomFileId = 1);

            var platformNumber = 1;
            var allCriteria = WatchForSearchCriteria();

            await Subject.PlatformSearch(_xemGame.Id, platformNumber, true, false, true, false);

            var criteria = allCriteria.OfType<PlatformSearchCriteria>().ToList();

            criteria.Count.Should().Be(0);
        }

        [Test]
        public async Task getscenenames_should_use_platformnumber_if_no_scene_platformnumber_is_available()
        {
            WithRoms();

            var allCriteria = WatchForSearchCriteria();

            await Subject.PlatformSearch(_xemGame.Id, 7, false, false, true, false);

            Mocker.GetMock<ISceneMappingService>()
                  .Verify(v => v.FindByIgdbId(_xemGame.Id), Times.Once());

            allCriteria.Should().HaveCount(1);
            allCriteria.First().Should().BeOfType<PlatformSearchCriteria>();
            allCriteria.First().As<PlatformSearchCriteria>().PlatformNumber.Should().Be(7);
        }

        [Test]
        public async Task rom_search_should_use_all_available_numbering_from_services_and_xem()
        {
            WithRom(1, 12, 2, 3);

            Mocker.GetMock<ISceneMappingService>()
                .Setup(s => s.FindByIgdbId(It.IsAny<int>()))
                .Returns(new List<SceneMapping>
                {
                    new SceneMapping
                    {
                        IgdbId = _xemGame.IgdbId,
                        SearchTerm = _xemGame.Title,
                        ParseTerm = _xemGame.Title,
                        FilterRegex = "(?i)-(BTN)$",
                        PlatformNumber = 1,
                        ScenePlatformNumber = 1,
                        SceneOrigin = "igdb",
                        Type = "ServicesProvider"
                    }
                });

            var allCriteria = WatchForSearchCriteria();

            await Subject.RomSearch(_xemRoms.First(), false, false);

            Mocker.GetMock<ISceneMappingService>()
                .Verify(v => v.FindByIgdbId(_xemGame.Id), Times.Once());

            allCriteria.Should().HaveCount(2);

            allCriteria.First().Should().BeOfType<SingleGameFileSearchCriteria>();
            allCriteria.First().As<SingleGameFileSearchCriteria>().PlatformNumber.Should().Be(1);
            allCriteria.First().As<SingleGameFileSearchCriteria>().FileNumber.Should().Be(12);

            allCriteria.Last().Should().BeOfType<SingleGameFileSearchCriteria>();
            allCriteria.Last().As<SingleGameFileSearchCriteria>().PlatformNumber.Should().Be(2);
            allCriteria.Last().As<SingleGameFileSearchCriteria>().FileNumber.Should().Be(3);
        }

        [Test]
        public async Task rom_search_should_include_series_title_when_not_a_direct_title_match()
        {
            _xemGame.Title = "Romarr's Title";
            _xemGame.CleanTitle = "romarrstitle";

            WithRom(1, 12, 2, 3);

            Mocker.GetMock<ISceneMappingService>()
                .Setup(s => s.FindByIgdbId(It.IsAny<int>()))
                .Returns(new List<SceneMapping>
                {
                    new SceneMapping
                    {
                        IgdbId = _xemGame.IgdbId,
                        SearchTerm = "Romarrs Title",
                        ParseTerm = _xemGame.CleanTitle,
                        PlatformNumber = 1,
                        ScenePlatformNumber = 1,
                        SceneOrigin = "igdb",
                        Type = "ServicesProvider"
                    }
                });

            var allCriteria = WatchForSearchCriteria();

            await Subject.RomSearch(_xemRoms.First(), false, false);

            Mocker.GetMock<ISceneMappingService>()
                .Verify(v => v.FindByIgdbId(_xemGame.Id), Times.Once());

            allCriteria.Should().HaveCount(2);

            allCriteria.First().Should().BeOfType<SingleGameFileSearchCriteria>();
            allCriteria.First().As<SingleGameFileSearchCriteria>().PlatformNumber.Should().Be(1);
            allCriteria.First().As<SingleGameFileSearchCriteria>().FileNumber.Should().Be(12);

            allCriteria.Last().Should().BeOfType<SingleGameFileSearchCriteria>();
            allCriteria.Last().As<SingleGameFileSearchCriteria>().PlatformNumber.Should().Be(2);
            allCriteria.Last().As<SingleGameFileSearchCriteria>().FileNumber.Should().Be(3);
        }
    }
}
