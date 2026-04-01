using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Romarr.Core.Datastore;
using Romarr.Core.DecisionEngine;
using Romarr.Core.Download;
using Romarr.Core.IndexerSearch;
using Romarr.Core.Messaging.Commands;
using Romarr.Core.Profiles.Qualities;
using Romarr.Core.Test.Framework;
using Romarr.Core.Games;

namespace Romarr.Core.Test.IndexerSearchTests
{
    [TestFixture]
    public class GameSearchServiceFixture : CoreTest<GameSearchService>
    {
        private Game _game;

        [SetUp]
        public void Setup()
        {
            _game = new Game
                      {
                          Id = 1,
                          Title = "Title",
                          Platforms = new List<Platform>(),
                          QualityProfile = new LazyLoaded<QualityProfile>(Builder<QualityProfile>.CreateNew().With(q => q.UpgradeAllowed = true).Build())
                      };

            Mocker.GetMock<IGameService>()
                  .Setup(s => s.GetGame(It.IsAny<int>()))
                  .Returns(_game);

            Mocker.GetMock<ISearchForReleases>()
                  .Setup(s => s.PlatformSearch(_game.Id, It.IsAny<int>(), false, false, true, false))
                  .Returns(Task.FromResult(new List<DownloadDecision>()));

            Mocker.GetMock<IProcessDownloadDecisions>()
                  .Setup(s => s.ProcessDecisions(It.IsAny<List<DownloadDecision>>()))
                  .Returns(Task.FromResult(new ProcessedDecisions(new List<DownloadDecision>(), new List<DownloadDecision>(), new List<DownloadDecision>())));
        }

        [Test]
        public void should_only_include_monitored_platforms()
        {
            _game.Platforms = new List<Platform>
                              {
                                  new Platform { PlatformNumber = 0, Monitored = false },
                                  new Platform { PlatformNumber = 1, Monitored = true }
                              };

            Subject.Execute(new GameSearchCommand { GameId = _game.Id, Trigger = CommandTrigger.Manual });

            Mocker.GetMock<ISearchForReleases>()
                .Verify(v => v.PlatformSearch(_game.Id, It.IsAny<int>(), false, true, true, false), Times.Exactly(_game.Platforms.Count(s => s.Monitored)));
        }

        [Test]
        public void should_only_search_missing_if_profile_does_not_allow_upgrades()
        {
            _game.Platforms = new List<Platform>
            {
                new Platform { PlatformNumber = 0, Monitored = false },
                new Platform { PlatformNumber = 1, Monitored = true }
            };

            _game.QualityProfile.Value.UpgradeAllowed = false;

            Subject.Execute(new GameSearchCommand { GameId = _game.Id, Trigger = CommandTrigger.Manual });

            Mocker.GetMock<ISearchForReleases>()
                .Verify(v => v.PlatformSearch(_game.Id, It.IsAny<int>(), true, true, true, false), Times.Exactly(_game.Platforms.Count(s => s.Monitored)));
        }

        [Test]
        public void should_start_with_lower_platforms_first()
        {
            var platformOrder = new List<int>();

            _game.Platforms = new List<Platform>
                              {
                                  new Platform { PlatformNumber = 3, Monitored = true },
                                  new Platform { PlatformNumber = 1, Monitored = true },
                                  new Platform { PlatformNumber = 2, Monitored = true }
                              };

            Mocker.GetMock<ISearchForReleases>()
                  .Setup(s => s.PlatformSearch(_game.Id, It.IsAny<int>(), false, true, true, false))
                  .Returns(Task.FromResult(new List<DownloadDecision>()))
                  .Callback<int, int, bool, bool, bool, bool>((gameId, platformNumber, missingOnly, monitoredOnly, userInvokedSearch, interactiveSearch) => platformOrder.Add(platformNumber));

            Subject.Execute(new GameSearchCommand { GameId = _game.Id, Trigger = CommandTrigger.Manual });

            platformOrder.First().Should().Be(_game.Platforms.OrderBy(s => s.PlatformNumber).First().PlatformNumber);
        }
    }
}
