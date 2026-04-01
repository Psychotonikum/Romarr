using System.Collections.Generic;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using Romarr.Core.History;
using Romarr.Core.Housekeeping.Housekeepers;
using Romarr.Core.Languages;
using Romarr.Core.Qualities;
using Romarr.Core.Test.Framework;
using Romarr.Core.Games;

namespace Romarr.Core.Test.Housekeeping.Housekeepers
{
    [TestFixture]
    public class CleanupOrphanedHistoryItemsFixture : DbTest<CleanupOrphanedHistoryItems, FileHistory>
    {
        private Game _series;
        private Rom _gameFile;

        [SetUp]
        public void Setup()
        {
            _series = Builder<Game>.CreateNew()
                                     .BuildNew();

            _gameFile = Builder<Rom>.CreateNew()
                                       .BuildNew();
        }

        private void GivenSeries()
        {
            Db.Insert(_series);
        }

        private void GivenGameFile()
        {
            Db.Insert(_gameFile);
        }

        [Test]
        public void should_delete_orphaned_items_by_series()
        {
            GivenGameFile();

            var history = Builder<FileHistory>.CreateNew()
                .With(h => h.Languages = new List<Language> { Language.English })
                .With(h => h.Quality = new QualityModel())
                .With(h => h.FileId = _gameFile.Id)
                .BuildNew();
            Db.Insert(history);

            Subject.Clean();
            AllStoredModels.Should().BeEmpty();
        }

        [Test]
        public void should_delete_orphaned_items_by_gameFile()
        {
            GivenSeries();

            var history = Builder<FileHistory>.CreateNew()
                .With(h => h.Languages = new List<Language> { Language.English })
                .With(h => h.Quality = new QualityModel())
                .With(h => h.GameId = _series.Id)
                .BuildNew();
            Db.Insert(history);

            Subject.Clean();
            AllStoredModels.Should().BeEmpty();
        }

        [Test]
        public void should_not_delete_unorphaned_data_by_series()
        {
            GivenSeries();
            GivenGameFile();

            var history = Builder<FileHistory>.CreateListOfSize(2)
                .All()
                .With(h => h.Languages = new List<Language> { Language.English })
                .With(h => h.Quality = new QualityModel())
                .With(h => h.FileId = _gameFile.Id)
                .TheFirst(1)
                .With(h => h.GameId = _series.Id)
                .BuildListOfNew();

            Db.InsertMany(history);

            Subject.Clean();
            AllStoredModels.Should().HaveCount(1);
            AllStoredModels.Should().Contain(h => h.GameId == _series.Id);
        }

        [Test]
        public void should_not_delete_unorphaned_data_by_gameFile()
        {
            GivenSeries();
            GivenGameFile();

            var history = Builder<FileHistory>.CreateListOfSize(2)
                .All()
                .With(h => h.Languages = new List<Language> { Language.English })
                .With(h => h.Quality = new QualityModel())
                .With(h => h.GameId = _series.Id)
                .TheFirst(1)
                .With(h => h.FileId = _gameFile.Id)
                .BuildListOfNew();

            Db.InsertMany(history);

            Subject.Clean();
            AllStoredModels.Should().HaveCount(1);
            AllStoredModels.Should().Contain(h => h.FileId == _gameFile.Id);
        }
    }
}
