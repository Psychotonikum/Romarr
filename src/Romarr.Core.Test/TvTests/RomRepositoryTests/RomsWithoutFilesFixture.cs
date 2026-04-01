using System;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using Romarr.Core.Datastore;
using Romarr.Core.Test.Framework;
using Romarr.Test.Common;
using Romarr.Core.Games;

namespace Romarr.Core.Test.TvTests.RomRepositoryTests
{
    [TestFixture]
    public class GameFilesWithoutFilesFixture : DbTest<RomRepository, Rom>
    {
        private Game _monitoredSeries;
        private Game _unmonitoredSeries;
        private PagingSpec<Rom> _pagingSpec;

        [SetUp]
        public void Setup()
        {
            _monitoredSeries = Builder<Game>.CreateNew()
                                        .With(s => s.Id = 0)
                                        .With(s => s.MobyGamesId = RandomNumber)
                                        .With(s => s.Runtime = 30)
                                        .With(s => s.Monitored = true)
                                        .With(s => s.TitleSlug = "Title3")
                                        .Build();

            _unmonitoredSeries = Builder<Game>.CreateNew()
                                        .With(s => s.Id = 0)
                                        .With(s => s.IgdbId = RandomNumber)
                                        .With(s => s.Runtime = 30)
                                        .With(s => s.Monitored = false)
                                        .With(s => s.TitleSlug = "Title2")
                                        .Build();

            _monitoredSeries.Id = Db.Insert(_monitoredSeries).Id;
            _unmonitoredSeries.Id = Db.Insert(_unmonitoredSeries).Id;

            _pagingSpec = new PagingSpec<Rom>
                              {
                                  Page = 1,
                                  PageSize = 10,
                                  SortKey = "AirDate",
                                  SortDirection = SortDirection.Ascending
                              };

            var monitoredSeriesGameFiles = Builder<Rom>.CreateListOfSize(3)
                                           .All()
                                           .With(e => e.Id = 0)
                                           .With(e => e.GameId = _monitoredSeries.Id)
                                           .With(e => e.RomFileId = 0)
                                           .With(e => e.AirDateUtc = DateTime.Now.AddDays(-5))
                                           .With(e => e.Monitored = true)
                                           .TheFirst(1)
                                           .With(e => e.Monitored = false)
                                           .TheLast(1)
                                           .With(e => e.PlatformNumber = 0)
                                           .Build();

            var unmonitoredSeriesGameFiles = Builder<Rom>.CreateListOfSize(3)
                                           .All()
                                           .With(e => e.Id = 0)
                                           .With(e => e.GameId = _unmonitoredSeries.Id)
                                           .With(e => e.RomFileId = 0)
                                           .With(e => e.AirDateUtc = DateTime.Now.AddDays(-5))
                                           .With(e => e.Monitored = true)
                                           .TheFirst(1)
                                           .With(e => e.Monitored = false)
                                           .TheLast(1)
                                           .With(e => e.PlatformNumber = 0)
                                           .Build();

            var unairedGameFiles           = Builder<Rom>.CreateListOfSize(1)
                                           .All()
                                           .With(e => e.Id = 0)
                                           .With(e => e.GameId = _monitoredSeries.Id)
                                           .With(e => e.RomFileId = 0)
                                           .With(e => e.AirDateUtc = DateTime.Now.AddDays(5))
                                           .With(e => e.Monitored = true)
                                           .Build();

            Db.InsertMany(monitoredSeriesGameFiles);
            Db.InsertMany(unmonitoredSeriesGameFiles);
            Db.InsertMany(unairedGameFiles);
        }

        private void GivenMonitoredFilterExpression()
        {
            _pagingSpec.FilterExpressions.Add(e => e.Monitored == true && e.Game.Monitored == true);
        }

        private void GivenUnmonitoredFilterExpression()
        {
            _pagingSpec.FilterExpressions.Add(e => e.Monitored == false || e.Game.Monitored == false);
        }

        [Test]
        public void should_get_monitored_gameFiles()
        {
            GivenMonitoredFilterExpression();

            var roms = Subject.GameFilesWithoutFiles(_pagingSpec, false);

            roms.Records.Should().HaveCount(1);
        }

        [Test]
        [Ignore("Specials not implemented")]
        public void should_get_gameFile_including_specials()
        {
            var roms = Subject.GameFilesWithoutFiles(_pagingSpec, true);

            roms.Records.Should().HaveCount(2);
        }

        [Test]
        public void should_not_include_unmonitored_gameFiles()
        {
            GivenMonitoredFilterExpression();

            var roms = Subject.GameFilesWithoutFiles(_pagingSpec, false);

            roms.Records.Should().NotContain(e => e.Monitored == false);
        }

        [Test]
        public void should_not_contain_unmonitored_series()
        {
            GivenMonitoredFilterExpression();

            var roms = Subject.GameFilesWithoutFiles(_pagingSpec, false);

            roms.Records.Should().NotContain(e => e.GameId == _unmonitoredSeries.Id);
        }

        [Test]
        public void should_not_return_unaired()
        {
            var roms = Subject.GameFilesWithoutFiles(_pagingSpec, false);

            roms.TotalRecords.Should().Be(4);
        }

        [Test]
        public void should_not_return_gameFiles_on_air()
        {
            ExceptionVerification.IgnoreErrors();
            var onAirGameFile = Builder<Rom>.CreateNew()
                                               .With(e => e.Id = 0)
                                               .With(e => e.GameId = _monitoredSeries.Id)
                                               .With(e => e.RomFileId = 0)
                                               .With(e => e.AirDateUtc = DateTime.Now.AddMinutes(-15))
                                               .With(e => e.Monitored = true)
                                               .Build();

            Db.Insert(onAirGameFile);

            var roms = Subject.GameFilesWithoutFiles(_pagingSpec, false);

            roms.TotalRecords.Should().Be(4);
            roms.Records.Where(e => e.Id == onAirGameFile.Id).Should().BeEmpty();
        }
    }
}
