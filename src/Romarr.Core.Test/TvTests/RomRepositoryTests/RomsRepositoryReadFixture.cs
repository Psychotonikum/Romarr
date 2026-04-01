using System.Collections.Generic;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using Romarr.Core.Languages;
using Romarr.Core.MediaFiles;
using Romarr.Core.Qualities;
using Romarr.Core.Test.Framework;
using Romarr.Core.Games;

namespace Romarr.Core.Test.TvTests.RomRepositoryTests
{
    [TestFixture]
    public class GameFilesRepositoryReadFixture : DbTest<RomRepository, Rom>
    {
        private Game _series;

        [SetUp]
        public void Setup()
        {
            _series = Builder<Game>.CreateNew()
                                        .With(s => s.Runtime = 30)
                                        .BuildNew();

            Db.Insert(_series);
        }

        [Test]
        public void should_get_gameFiles_by_file()
        {
            var romFile = Builder<RomFile>.CreateNew()
                .With(h => h.Quality = new QualityModel())
                .With(h => h.Languages = new List<Language> { Language.English })
                .BuildNew();

            Db.Insert(romFile);

            var rom = Builder<Rom>.CreateListOfSize(2)
                                        .All()
                                        .With(e => e.GameId = _series.Id)
                                        .With(e => e.RomFileId = romFile.Id)
                                        .BuildListOfNew();

            Db.InsertMany(rom);

            var roms = Subject.GetGameFileByFileId(romFile.Id);
            roms.Should().HaveCount(2);
        }
    }
}
