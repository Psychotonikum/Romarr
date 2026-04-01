using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using Romarr.Core.History;
using Romarr.Core.Languages;
using Romarr.Core.Qualities;
using Romarr.Core.Test.Framework;
using Romarr.Core.Games;

namespace Romarr.Core.Test.HistoryTests
{
    [TestFixture]
    public class HistoryRepositoryFixture : DbTest<HistoryRepository, FileHistory>
    {
        private Game _series1;
        private Game _series2;

        [SetUp]
        public void Setup()
        {
            _series1 = Builder<Game>.CreateNew()
                                      .With(s => s.Id = 7)
                                      .Build();

            _series2 = Builder<Game>.CreateNew()
                                      .With(s => s.Id = 8)
                                      .Build();
        }

        [Test]
        public void should_read_write_dictionary()
        {
            var history = Builder<FileHistory>.CreateNew()
                .With(c => c.Languages = new List<Language> { Language.English })
                .With(c => c.Quality = new QualityModel())
                .BuildNew();

            history.Data.Add("key1", "value1");
            history.Data.Add("key2", "value2");

            Subject.Insert(history);

            StoredModel.Data.Should().HaveCount(2);
        }

        [Test]
        public void should_get_download_history()
        {
            var historyBluray = Builder<FileHistory>.CreateNew()
                .With(c => c.Languages = new List<Language> { Language.English })
                .With(c => c.Quality = new QualityModel(Quality.Verified))
                .With(c => c.GameId = 12)
                .With(c => c.EventType = FileHistoryEventType.Grabbed)
                .BuildNew();

            var historyDvd = Builder<FileHistory>.CreateNew()
                .With(c => c.Languages = new List<Language> { Language.English })
                .With(c => c.Quality = new QualityModel(Quality.Bad))
                .With(c => c.GameId = 12)
                .With(c => c.EventType = FileHistoryEventType.Grabbed)
             .BuildNew();

            Subject.Insert(historyBluray);
            Subject.Insert(historyDvd);

            var downloadHistory = Subject.FindDownloadHistory(12, new QualityModel(Quality.Verified));

            downloadHistory.Should().HaveCount(1);
        }

        [Test]
        public void should_delete_history_items_by_gameId()
        {
            var items = Builder<FileHistory>.CreateListOfSize(5)
                .TheFirst(1)
                .With(c => c.GameId = _series2.Id)
                .TheRest()
                .With(c => c.GameId = _series1.Id)
                .All()
                .With(c => c.Id = 0)
                .With(c => c.Quality = new QualityModel(Quality.Bluray1080p))
                .With(c => c.Languages = new List<Language> { Language.English })
                .With(c => c.EventType = FileHistoryEventType.Grabbed)
                .BuildListOfNew();

            Db.InsertMany(items);

            Subject.DeleteForSeries(new List<int> { _series1.Id });

            var dbItems = Subject.All();
            var removedItems = dbItems.Where(h => h.GameId == _series1.Id);
            var nonRemovedItems = dbItems.Where(h => h.GameId == _series2.Id);

            removedItems.Should().HaveCount(0);
            nonRemovedItems.Should().HaveCount(1);
        }
    }
}
