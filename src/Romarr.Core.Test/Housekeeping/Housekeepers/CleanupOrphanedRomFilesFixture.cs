using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using Romarr.Core.Housekeeping.Housekeepers;
using Romarr.Core.Languages;
using Romarr.Core.MediaFiles;
using Romarr.Core.Qualities;
using Romarr.Core.Test.Framework;
using Romarr.Core.Games;

namespace Romarr.Core.Test.Housekeeping.Housekeepers
{
    [TestFixture]
    public class CleanupOrphanedRomFilesFixture : DbTest<CleanupOrphanedRomFiles, RomFile>
    {
        [Test]
        public void should_delete_orphaned_gameFile_files()
        {
            var romFile = Builder<RomFile>.CreateNew()
                .With(h => h.Languages = new List<Language> { Language.English })
                .With(h => h.Quality = new QualityModel())
                .BuildNew();

            Db.Insert(romFile);
            Subject.Clean();
            AllStoredModels.Should().BeEmpty();
        }

        [Test]
        public void should_not_delete_unorphaned_gameFile_files()
        {
            var romFiles = Builder<RomFile>.CreateListOfSize(2)
                .All()
                .With(h => h.Languages = new List<Language> { Language.English })
                .With(h => h.Quality = new QualityModel())
                .BuildListOfNew();

            Db.InsertMany(romFiles);

            var rom = Builder<Rom>.CreateNew()
                .With(e => e.RomFileId = romFiles.First().Id)
                .BuildNew();

            Db.Insert(rom);

            Subject.Clean();
            AllStoredModels.Should().HaveCount(1);
            Db.All<Rom>().Should().Contain(e => e.RomFileId == AllStoredModels.First().Id);
        }
    }
}
