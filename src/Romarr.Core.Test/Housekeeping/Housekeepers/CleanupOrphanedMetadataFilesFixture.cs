using System.Collections.Generic;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using Romarr.Core.Extras.Metadata;
using Romarr.Core.Extras.Metadata.Files;
using Romarr.Core.Housekeeping.Housekeepers;
using Romarr.Core.Languages;
using Romarr.Core.MediaFiles;
using Romarr.Core.Qualities;
using Romarr.Core.Test.Framework;
using Romarr.Core.Games;

namespace Romarr.Core.Test.Housekeeping.Housekeepers
{
    [TestFixture]
    public class CleanupOrphanedMetadataFilesFixture : DbTest<CleanupOrphanedMetadataFiles, MetadataFile>
    {
        [Test]
        public void should_delete_metadata_files_that_dont_have_a_coresponding_series()
        {
            var metadataFile = Builder<MetadataFile>.CreateNew()
                                                    .With(m => m.RomFileId = null)
                                                    .BuildNew();

            Db.Insert(metadataFile);
            Subject.Clean();
            AllStoredModels.Should().BeEmpty();
        }

        [Test]
        public void should_not_delete_metadata_files_that_have_a_coresponding_series()
        {
            var game = Builder<Game>.CreateNew()
                                        .BuildNew();

            Db.Insert(game);

            var metadataFile = Builder<MetadataFile>.CreateNew()
                                                    .With(m => m.GameId = game.Id)
                                                    .With(m => m.RomFileId = null)
                                                    .BuildNew();

            Db.Insert(metadataFile);
            Subject.Clean();
            AllStoredModels.Should().HaveCount(1);
        }

        [Test]
        public void should_delete_metadata_files_that_dont_have_a_coresponding_gameFile_file()
        {
            var game = Builder<Game>.CreateNew()
                                        .BuildNew();

            Db.Insert(game);

            var metadataFile = Builder<MetadataFile>.CreateNew()
                                                    .With(m => m.GameId = game.Id)
                                                    .With(m => m.RomFileId = 10)
                                                    .BuildNew();

            Db.Insert(metadataFile);
            Subject.Clean();
            AllStoredModels.Should().BeEmpty();
        }

        [Test]
        public void should_not_delete_metadata_files_that_have_a_coresponding_gameFile_file()
        {
            var game = Builder<Game>.CreateNew()
                                        .BuildNew();

            var romFile = Builder<RomFile>.CreateNew()
                .With(h => h.Quality = new QualityModel())
                .With(h => h.Languages = new List<Language> { Language.English })
                .BuildNew();

            Db.Insert(game);
            Db.Insert(romFile);

            var metadataFile = Builder<MetadataFile>.CreateNew()
                                                    .With(m => m.GameId = game.Id)
                                                    .With(m => m.RomFileId = romFile.Id)
                                                    .BuildNew();

            Db.Insert(metadataFile);
            Subject.Clean();
            AllStoredModels.Should().HaveCount(1);
        }

        [Test]
        public void should_delete_gameFile_metadata_files_that_have_gameFilefileid_of_zero()
        {
            var game = Builder<Game>.CreateNew()
                                        .BuildNew();

            Db.Insert(game);

            var metadataFile = Builder<MetadataFile>.CreateNew()
                                                 .With(m => m.GameId = game.Id)
                                                 .With(m => m.Type = MetadataType.GameFileMetadata)
                                                 .With(m => m.RomFileId = 0)
                                                 .BuildNew();

            Db.Insert(metadataFile);
            Subject.Clean();
            AllStoredModels.Should().HaveCount(0);
        }

        [Test]
        public void should_delete_gameFile_image_files_that_have_gameFilefileid_of_zero()
        {
            var game = Builder<Game>.CreateNew()
                                        .BuildNew();

            Db.Insert(game);

            var metadataFile = Builder<MetadataFile>.CreateNew()
                                                    .With(m => m.GameId = game.Id)
                                                    .With(m => m.Type = MetadataType.GameFileImage)
                                                    .With(m => m.RomFileId = 0)
                                                    .BuildNew();

            Db.Insert(metadataFile);
            Subject.Clean();
            AllStoredModels.Should().HaveCount(0);
        }
    }
}
