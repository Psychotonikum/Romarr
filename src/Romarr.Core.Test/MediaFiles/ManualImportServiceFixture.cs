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

            var item = Subject.ReprocessItem(Path.Combine(TempFolder, "Game.S01E01.mkv"), null, game.Id, 2,
                new List<int>(), null, new QualityModel(Quality.Unknown), new List<Language> { Language.Unknown }, 0, ReleaseType.Unknown);

            item.PlatformNumber.Should().Be(2);
        }
    }
}
