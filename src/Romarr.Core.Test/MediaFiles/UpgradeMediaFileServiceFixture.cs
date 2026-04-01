using System.IO;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Romarr.Common.Disk;
using Romarr.Core.Datastore;
using Romarr.Core.MediaFiles;
using Romarr.Core.MediaFiles.GameFileImport;
using Romarr.Core.Parser.Model;
using Romarr.Core.Test.Framework;
using Romarr.Core.Games;
using Romarr.Test.Common;

namespace Romarr.Core.Test.MediaFiles
{
    public class UpgradeMediaFileServiceFixture : CoreTest<UpgradeMediaFileService>
    {
        private RomFile _romFile;
        private LocalGameFile _localRom;

        [SetUp]
        public void Setup()
        {
            _localRom = new LocalGameFile();
            _localRom.Game = new Game
                                   {
                                       Path = @"C:\Test\TV\Game".AsOsAgnostic()
                                   };

            _romFile = Builder<RomFile>
                .CreateNew()
                .Build();

            Mocker.GetMock<IDiskProvider>()
                  .Setup(c => c.FolderExists(Directory.GetParent(_localRom.Game.Path).FullName))
                  .Returns(true);

            Mocker.GetMock<IDiskProvider>()
                  .Setup(c => c.FileExists(It.IsAny<string>()))
                  .Returns(true);

            Mocker.GetMock<IDiskProvider>()
                  .Setup(c => c.GetParentFolder(It.IsAny<string>()))
                  .Returns<string>(c => Path.GetDirectoryName(c));
        }

        private void GivenSingleGameFileWithSingleRomFile()
        {
            _localRom.Roms = Builder<Rom>.CreateListOfSize(1)
                                                     .All()
                                                     .With(e => e.RomFileId = 1)
                                                     .With(e => e.RomFile = new RomFile
                                                                                {
                                                                                    Id = 1,
                                                                                    RelativePath = @"Platform 01\30.rock.s01e01.avi",
                                                                                })
                                                     .Build()
                                                     .ToList();
        }

        private void GivenMultipleGameFilesWithSingleRomFile()
        {
            _localRom.Roms = Builder<Rom>.CreateListOfSize(2)
                                                     .All()
                                                     .With(e => e.RomFileId = 1)
                                                     .With(e => e.RomFile = new RomFile
                                                                                {
                                                                                    Id = 1,
                                                                                    RelativePath = @"Platform 01\30.rock.s01e01.avi",
                                                                                })
                                                     .Build()
                                                     .ToList();
        }

        private void GivenMultipleGameFilesWithMultipleRomFiles()
        {
            _localRom.Roms = Builder<Rom>.CreateListOfSize(2)
                                                     .TheFirst(1)
                                                     .With(e => e.RomFile = new RomFile
                                                                                {
                                                                                    Id = 1,
                                                                                    RelativePath = @"Platform 01\30.rock.s01e01.avi",
                                                                                })
                                                     .TheNext(1)
                                                     .With(e => e.RomFile = new RomFile
                                                                                {
                                                                                    Id = 2,
                                                                                    RelativePath = @"Platform 01\30.rock.s01e02.avi",
                                                                                })
                                                     .Build()
                                                     .ToList();
        }

        [Test]
        public void should_delete_single_gameFile_file_once()
        {
            GivenSingleGameFileWithSingleRomFile();

            Subject.UpgradeRomFile(_romFile, _localRom);

            Mocker.GetMock<IRecycleBinProvider>().Verify(v => v.DeleteFile(It.IsAny<string>(), It.IsAny<string>()), Times.Once());
        }

        [Test]
        public void should_delete_the_same_gameFile_file_only_once()
        {
            GivenMultipleGameFilesWithSingleRomFile();

            Subject.UpgradeRomFile(_romFile, _localRom);

            Mocker.GetMock<IRecycleBinProvider>().Verify(v => v.DeleteFile(It.IsAny<string>(), It.IsAny<string>()), Times.Once());
        }

        [Test]
        public void should_delete_multiple_different_gameFile_files()
        {
            GivenMultipleGameFilesWithMultipleRomFiles();

            Subject.UpgradeRomFile(_romFile, _localRom);

            Mocker.GetMock<IRecycleBinProvider>().Verify(v => v.DeleteFile(It.IsAny<string>(), It.IsAny<string>()), Times.Exactly(2));
        }

        [Test]
        public void should_delete_gameFile_file_from_database()
        {
            GivenSingleGameFileWithSingleRomFile();

            Subject.UpgradeRomFile(_romFile, _localRom);

            Mocker.GetMock<IMediaFileService>().Verify(v => v.Delete(It.IsAny<RomFile>(), DeleteMediaFileReason.Upgrade), Times.Once());
        }

        [Test]
        public void should_delete_existing_file_fromdb_if_file_doesnt_exist()
        {
            GivenSingleGameFileWithSingleRomFile();

            Mocker.GetMock<IDiskProvider>()
                .Setup(c => c.FileExists(It.IsAny<string>()))
                .Returns(false);

            Subject.UpgradeRomFile(_romFile, _localRom);

            Mocker.GetMock<IMediaFileService>().Verify(v => v.Delete(_localRom.Roms.Single().RomFile, DeleteMediaFileReason.Upgrade), Times.Once());

            ExceptionVerification.ExpectedWarns(1);
        }

        [Test]
        public void should_not_try_to_recyclebin_existing_file_if_file_doesnt_exist()
        {
            GivenSingleGameFileWithSingleRomFile();

            Mocker.GetMock<IDiskProvider>()
                .Setup(c => c.FileExists(It.IsAny<string>()))
                .Returns(false);

            Subject.UpgradeRomFile(_romFile, _localRom);

            Mocker.GetMock<IRecycleBinProvider>().Verify(v => v.DeleteFile(It.IsAny<string>(), It.IsAny<string>()), Times.Never());

            ExceptionVerification.ExpectedWarns(1);
        }

        [Test]
        public void should_return_old_gameFile_file_in_oldFiles()
        {
            GivenSingleGameFileWithSingleRomFile();

            Subject.UpgradeRomFile(_romFile, _localRom).OldFiles.Count.Should().Be(1);
        }

        [Test]
        public void should_return_old_gameFile_files_in_oldFiles()
        {
            GivenMultipleGameFilesWithMultipleRomFiles();

            Subject.UpgradeRomFile(_romFile, _localRom).OldFiles.Count.Should().Be(2);
        }

        [Test]
        public void should_throw_if_there_are_existing_gameFile_files_and_the_root_folder_is_missing()
        {
            GivenSingleGameFileWithSingleRomFile();

            Mocker.GetMock<IDiskProvider>()
                  .Setup(c => c.FolderExists(Directory.GetParent(_localRom.Game.Path).FullName))
                  .Returns(false);

            Assert.Throws<RootFolderNotFoundException>(() => Subject.UpgradeRomFile(_romFile, _localRom));

            Mocker.GetMock<IMediaFileService>().Verify(v => v.Delete(_localRom.Roms.Single().RomFile, DeleteMediaFileReason.Upgrade), Times.Never());
        }

        [Test]
        public void should_import_if_existing_file_doesnt_exist_in_db()
        {
            _localRom.Roms = Builder<Rom>.CreateListOfSize(1)
                                                     .All()
                                                     .With(e => e.RomFileId = 1)
                                                     .With(e => e.RomFile = new LazyLoaded<RomFile>(null))
                                                     .Build()
                                                     .ToList();

            Subject.UpgradeRomFile(_romFile, _localRom);

            Mocker.GetMock<IMediaFileService>().Verify(v => v.Delete(_localRom.Roms.Single().RomFile, It.IsAny<DeleteMediaFileReason>()), Times.Never());
        }
    }
}
