using System.Collections.Generic;
using System.IO;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Romarr.Common.Disk;
using Romarr.Core.Languages;
using Romarr.Core.MediaFiles;
using Romarr.Core.MediaFiles.GameFileImport;
using Romarr.Core.MediaFiles.GameFileImport.Manual;
using Romarr.Core.Parser.Model;
using Romarr.Core.Qualities;
using Romarr.Core.Test.Framework;
using Romarr.Core.Games;

namespace Romarr.Core.Test.MediaFiles
{
    [TestFixture]
    public class ManualImportServiceFixture : CoreTest<ManualImportService>
    {
        [TestCase("Game.S02E01.mkv", 2, false)]
        [TestCase("Game.S03E01.mkv", null, false)]
        [TestCase("Game.S03E01.mkv", null, true)]
        [TestCase("Game.mkv", null, false)]
        [TestCase("Game.mkv", 1, true)]
        [TestCase("Game.gba", 2, false)]
        [TestCase("Game.gb", 1, false)]
        [TestCase("Game Boy Advance/Game.mkv", 2, false)]
        public void should_detect_platform_only_when_unambiguous(string name, int? expectedPlatform, bool singlePlatform)
        {
            var path = Path.Combine(TempFolder, name);
            var game = new Game
            {
                Path = TempFolder,
                Platforms = new List<Platform> { new Platform { PlatformNumber = 1, Title = "Game Boy" } }
            };

            if (!singlePlatform)
            {
                game.Platforms.Add(new Platform { PlatformNumber = 2, Title = "Game Boy Advance" });
            }

            var localRom = new LocalGameFile { Game = game, Path = path };

            Mocker.GetMock<IDiskProvider>().Setup(disk => disk.FolderExists(TempFolder)).Returns(true);
            Mocker.GetMock<IGameService>().Setup(service => service.GetGame(game.Id)).Returns(game);
            Mocker.GetMock<IDiskScanService>().Setup(service => service.GetVideoFiles(TempFolder, true)).Returns(new[] { path });
            Mocker.GetMock<IDiskScanService>()
                .Setup(service => service.FilterPaths(TempFolder, It.IsAny<IEnumerable<string>>(), true))
                .Returns(new List<string> { path });
            Mocker.GetMock<IMakeImportDecision>()
                .Setup(service => service.GetImportDecisions(It.IsAny<List<string>>(), game, null, null, It.IsAny<ParsedRomInfo>(), false, false))
                .Returns(new List<ImportDecision> { new ImportDecision(localRom) });

            var item = Subject.GetMediaFiles(TempFolder, null, game.Id, false).Should().ContainSingle().Subject;

            item.PlatformNumber.Should().Be(expectedPlatform);
            Mocker.GetMock<ILocalGameFileCustomFormatCalculationService>()
                .Verify(service => service.ParseGameFileCustomFormats(It.IsAny<LocalGameFile>()), Times.Never());
        }

        [Test]
        public void should_keep_selected_platform_while_roms_are_unselected()
        {
            var game = new Game
            {
                Path = TempFolder,
                Platforms = new List<Platform>
                {
                    new Platform { PlatformNumber = 1 },
                    new Platform { PlatformNumber = 2 }
                }
            };

            Mocker.GetMock<IGameService>().Setup(service => service.GetGame(game.Id)).Returns(game);

            var item = Subject.ReprocessItem(Path.Combine(TempFolder, "Game.S01E01.mkv"),
                                             null,
                                             game.Id,
                                             2,
                                             new List<int>(),
                                             null,
                                             null,
                                             new List<Language> { Language.Unknown },
                                             0,
                                             ReleaseType.Unknown);

            item.PlatformNumber.Should().Be(2);
            item.Quality.Quality.Should().Be(Quality.Unknown);
        }

        [TestCase(1, 2)]
        [TestCase(2, 3)]
        public void should_match_switch_dlc_pack_to_its_platform_rom(int packNumber, int expectedFileNumber)
        {
            var game = new Game
            {
                Path = TempFolder,
                Platforms = new List<Platform>
                {
                    new Platform { PlatformNumber = 1, Title = "Wii U" },
                    new Platform { PlatformNumber = 2, Title = "Nintendo Switch" }
                }
            };
            var path = Path.Combine(TempFolder, $"DLC/Game [01007EF00011E000] - Game DLC Pack {packNumber} [01007EF00011F001] [DLC][v196608].nsz");
            var localRom = new LocalGameFile { Game = game, Path = path };

            Mocker.GetMock<IDiskProvider>().Setup(disk => disk.FolderExists(TempFolder)).Returns(true);
            Mocker.GetMock<IGameService>().Setup(service => service.GetGame(game.Id)).Returns(game);
            Mocker.GetMock<IDiskScanService>().Setup(service => service.GetVideoFiles(TempFolder, true)).Returns(new[] { path });
            Mocker.GetMock<IDiskScanService>()
                .Setup(service => service.FilterPaths(TempFolder, It.IsAny<IEnumerable<string>>(), true))
                .Returns(new List<string> { path });
            Mocker.GetMock<IMakeImportDecision>()
                .Setup(service => service.GetImportDecisions(It.IsAny<List<string>>(), game, null, null, It.IsAny<ParsedRomInfo>(), false, false))
                .Returns(new List<ImportDecision>
                {
                    new ImportDecision(localRom,
                                       new ImportRejection(ImportRejectionReason.UnableToParse, "Unable to parse file"),
                                       new ImportRejection(ImportRejectionReason.FileLocked, "File locked"))
                });
            Mocker.GetMock<IRomService>()
                .Setup(service => service.GetRomsByPlatform(game.Id, 2))
                .Returns(new List<Rom>
                {
                    new Rom { PlatformNumber = 2, FileNumber = 1, RomType = RomFileType.Base },
                    new Rom { PlatformNumber = 2, FileNumber = 2, RomType = RomFileType.Dlc },
                    new Rom { PlatformNumber = 2, FileNumber = 3, RomType = RomFileType.Dlc }
                });

            var item = Subject.GetMediaFiles(TempFolder, null, game.Id, false).Should().ContainSingle().Subject;

            item.PlatformNumber.Should().Be(2);
            Mocker.GetMock<IRomService>().Verify(service => service.GetRomsByPlatform(game.Id, 2), Times.Once());
            item.Roms.Should().ContainSingle().Which.FileNumber.Should().Be(expectedFileNumber);
            item.ReleaseType.Should().Be(ReleaseType.SingleGameFile);
            item.Quality.Quality.Should().Be(Quality.Unknown);
            item.Rejections.Should().ContainSingle().Which.Reason.Should().Be(ImportRejectionReason.FileLocked);
            Mocker.GetMock<ILocalGameFileCustomFormatCalculationService>()
                .Verify(service => service.ParseGameFileCustomFormats(It.Is<LocalGameFile>(local => local.Roms.Count == 1)), Times.Once());
            Mocker.GetMock<IRomService>().Verify(service => service.GetRomsByPlatform(game.Id, 1), Times.Never());

            Mocker.GetMock<IMediaFileService>().Setup(service => service.GetFilesBySeries(game.Id)).Returns(new List<RomFile>());
            Mocker.GetMock<IRomService>().Setup(service => service.GetGameFileBySeries(game.Id)).Returns(new List<Rom>());

            var libraryItem = Subject.GetMediaFiles(game.Id, null).Should().ContainSingle().Subject;

            libraryItem.PlatformNumber.Should().Be(2);
            libraryItem.Roms.Should().ContainSingle().Which.FileNumber.Should().Be(expectedFileNumber);
            libraryItem.ReleaseType.Should().Be(ReleaseType.SingleGameFile);
        }

        [TestCase("Game [01007EF00011E000] [BASE].xci", 1)]
        [TestCase("Updates/Game [01007EF00011E000] [UPDATE][v1114112].nsz", 4)]
        public void should_match_switch_base_and_versioned_update(string name, int expectedFileNumber)
        {
            var game = new Game
            {
                Path = TempFolder,
                Platforms = new List<Platform>
                {
                    new Platform { PlatformNumber = 1, Title = "Wii U" },
                    new Platform { PlatformNumber = 2, Title = "Nintendo Switch" }
                }
            };
            var path = Path.Combine(TempFolder, name);
            var localRom = new LocalGameFile { Game = game, Path = path };

            Mocker.GetMock<IDiskProvider>().Setup(disk => disk.FolderExists(TempFolder)).Returns(true);
            Mocker.GetMock<IGameService>().Setup(service => service.GetGame(game.Id)).Returns(game);
            Mocker.GetMock<IDiskScanService>().Setup(service => service.GetVideoFiles(TempFolder, true)).Returns(new[] { path });
            Mocker.GetMock<IDiskScanService>()
                .Setup(service => service.FilterPaths(TempFolder, It.IsAny<IEnumerable<string>>(), true))
                .Returns(new List<string> { path });
            Mocker.GetMock<IMakeImportDecision>()
                .Setup(service => service.GetImportDecisions(It.IsAny<List<string>>(), game, null, null, It.IsAny<ParsedRomInfo>(), false, false))
                .Returns(new List<ImportDecision> { new ImportDecision(localRom, new ImportRejection(ImportRejectionReason.UnableToParse, "Unable to parse file")) });
            Mocker.GetMock<IRomService>()
                .Setup(service => service.GetRomsByPlatform(game.Id, 2))
                .Returns(new List<Rom>
                {
                    new Rom { PlatformNumber = 2, FileNumber = 1, RomType = RomFileType.Base },
                    new Rom { PlatformNumber = 2, FileNumber = 4, RomType = RomFileType.Update, Title = "Game v1114112" },
                    new Rom { PlatformNumber = 2, FileNumber = 5, RomType = RomFileType.Update, Title = "Game v11141120" }
                });

            var item = Subject.GetMediaFiles(TempFolder, null, game.Id, false).Should().ContainSingle().Subject;

            item.PlatformNumber.Should().Be(2);
            item.Roms.Should().ContainSingle().Which.FileNumber.Should().Be(expectedFileNumber);
            item.Rejections.Should().BeEmpty();
            item.ReleaseType.Should().Be(ReleaseType.SingleGameFile);
            Mocker.GetMock<IRomService>().Verify(service => service.GetRomsByPlatform(game.Id, 1), Times.Never());
        }
    }
}
