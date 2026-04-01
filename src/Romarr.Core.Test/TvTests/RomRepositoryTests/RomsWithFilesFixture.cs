using System.Collections.Generic;
using System.Linq;
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
    public class GameFilesWithFilesFixture : DbTest<RomRepository, Rom>
    {
        private const int SERIES_ID = 1;
        private List<Rom> _gameFiles;
        private List<RomFile> _romFiles;

        [SetUp]
        public void Setup()
        {
            _romFiles = Builder<RomFile>.CreateListOfSize(5)
                                                .All()
                                                .With(c => c.Quality = new QualityModel())
                                                .With(c => c.Languages = new List<Language> { Language.English })
                                                .BuildListOfNew();

            Db.InsertMany(_romFiles);

            _gameFiles = Builder<Rom>.CreateListOfSize(10)
                                        .All()
                                        .With(e => e.RomFileId = 0)
                                        .With(e => e.GameId = SERIES_ID)
                                        .BuildListOfNew()
                                        .ToList();

            for (var i = 0; i < _romFiles.Count; i++)
            {
                _gameFiles[i].RomFileId = _romFiles[i].Id;
            }

            Db.InsertMany(_gameFiles);
        }

        [Test]
        public void should_only_get_files_that_have_gameFile_files()
        {
            var result = Subject.GameFilesWithFiles(SERIES_ID);

            result.Should().OnlyContain(e => e.RomFileId > 0);
            result.Should().HaveCount(_romFiles.Count);
        }

        [Test]
        public void should_only_contain_gameFiles_for_the_given_series()
        {
            var romFile = Builder<RomFile>.CreateNew()
                                                  .With(f => f.RelativePath = "another path")
                                                  .With(c => c.Quality = new QualityModel())
                                                  .With(c => c.Languages = new List<Language> { Language.English })
                                                  .BuildNew();

            Db.Insert(romFile);

            var rom = Builder<Rom>.CreateNew()
                                          .With(e => e.GameId = SERIES_ID + 10)
                                          .With(e => e.RomFileId = romFile.Id)
                                          .BuildNew();

            Db.Insert(rom);

            Subject.GameFilesWithFiles(rom.GameId).Should().OnlyContain(e => e.GameId == rom.GameId);
        }

        [Test]
        public void should_have_gameFile_file_loaded()
        {
            Subject.GameFilesWithFiles(SERIES_ID).Should().OnlyContain(e => e.RomFile.IsLoaded);
        }
    }
}
