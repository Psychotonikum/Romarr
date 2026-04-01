using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using Romarr.Core.MediaFiles.GameFileImport.Aggregation.Aggregators;
using Romarr.Core.Parser.Model;
using Romarr.Core.Test.Framework;
using Romarr.Core.Games;
using Romarr.Test.Common;

namespace Romarr.Core.Test.MediaFiles.GameFileImport.Aggregation.Aggregators
{
    [TestFixture]
    public class AggregateReleaseGroupFixture : CoreTest<AggregateReleaseGroup>
    {
        private Game _series;

        [SetUp]
        public void Setup()
        {
            _series = Builder<Game>.CreateNew().Build();
        }

        [Test]
        public void should_not_use_downloadclient_for_full_platform()
        {
            var fileRomInfo = Parser.Parser.ParseTitle("Game.Title.S01E01.WEB-DL-Wizzy");
            var downloadClientRomInfo = Parser.Parser.ParseTitle("Game.Title.S01.WEB-DL-Viva");
            var localRom = new LocalGameFile
            {
                FileRomInfo = fileRomInfo,
                DownloadClientRomInfo = downloadClientRomInfo,
                Path = @"C:\Test\Unsorted TV\Game.Title.S01\Game.Title.S01E01.WEB-DL.mkv".AsOsAgnostic(),
                Game = _series
            };

            Subject.Aggregate(localRom, null);

            localRom.ReleaseGroup.Should().Be("Wizzy");
        }

        [Test]
        public void should_not_use_folder_for_full_platform()
        {
            var fileRomInfo = Parser.Parser.ParseTitle("Game.Title.S01E01.WEB-DL-Wizzy");
            var folderRomInfo = Parser.Parser.ParseTitle("Game.Title.S01.WEB-DL-Drone");
            var localRom = new LocalGameFile
            {
                FileRomInfo = fileRomInfo,
                FolderRomInfo = folderRomInfo,
                Path = @"C:\Test\Unsorted TV\Game.Title.S01\Game.Title.S01E01.WEB-DL.mkv".AsOsAgnostic(),
                Game = _series
            };

            Subject.Aggregate(localRom, null);

            localRom.ReleaseGroup.Should().Be("Wizzy");
        }

        [Test]
        public void should_prefer_downloadclient()
        {
            var fileRomInfo = Parser.Parser.ParseTitle("Game.Title.S01E01.WEB-DL-Wizzy");
            var folderRomInfo = Parser.Parser.ParseTitle("Game.Title.S01E01.WEB-DL-Drone");
            var downloadClientRomInfo = Parser.Parser.ParseTitle("Game.Title.S01E01.WEB-DL-Viva");
            var localRom = new LocalGameFile
            {
                FileRomInfo = fileRomInfo,
                FolderRomInfo = folderRomInfo,
                DownloadClientRomInfo = downloadClientRomInfo,
                Path = @"C:\Test\Unsorted TV\Game.Title.S01\Game.Title.S01E01.WEB-DL.mkv".AsOsAgnostic(),
                Game = _series
            };

            Subject.Aggregate(localRom, null);

            localRom.ReleaseGroup.Should().Be("Viva");
        }

        [Test]
        public void should_prefer_folder()
        {
            var fileRomInfo = Parser.Parser.ParseTitle("Game.Title.S01E01.WEB-DL-Wizzy");
            var folderRomInfo = Parser.Parser.ParseTitle("Game.Title.S01E01.WEB-DL-Drone");
            var downloadClientRomInfo = Parser.Parser.ParseTitle("Game.Title.S01E01.WEB-DL");
            var localRom = new LocalGameFile
            {
                FileRomInfo = fileRomInfo,
                FolderRomInfo = folderRomInfo,
                DownloadClientRomInfo = downloadClientRomInfo,
                Path = @"C:\Test\Unsorted TV\Game.Title.S01\Game.Title.S01E01.WEB-DL.mkv".AsOsAgnostic(),
                Game = _series
            };

            Subject.Aggregate(localRom, null);

            localRom.ReleaseGroup.Should().Be("Drone");
        }

        [Test]
        public void should_fallback_to_file()
        {
            var fileRomInfo = Parser.Parser.ParseTitle("Game.Title.S01E01.WEB-DL-Wizzy");
            var folderRomInfo = Parser.Parser.ParseTitle("Game.Title.S01E01.WEB-DL");
            var downloadClientRomInfo = Parser.Parser.ParseTitle("Game.Title.S01E01.WEB-DL");
            var localRom = new LocalGameFile
            {
                FileRomInfo = fileRomInfo,
                FolderRomInfo = folderRomInfo,
                DownloadClientRomInfo = downloadClientRomInfo,
                Path = @"C:\Test\Unsorted TV\Game.Title.S01\Game.Title.S01E01.mkv".AsOsAgnostic(),
                Game = _series
            };

            Subject.Aggregate(localRom, null);

            localRom.ReleaseGroup.Should().Be("Wizzy");
        }
    }
}
