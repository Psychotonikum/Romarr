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
    public class AggregateReleaseHashFixture : CoreTest<AggregateReleaseHash>
    {
        private Game _series;

        [SetUp]
        public void Setup()
        {
            _series = Builder<Game>.CreateNew().Build();
        }

        [Test]
        public void should_prefer_file()
        {
            var fileRomInfo = Parser.Parser.ParseTitle("[DHD] Game Title! - 08 (1280x720 10bit AAC) [ABCDEFGH]");
            var folderRomInfo = Parser.Parser.ParseTitle("[DHD] Game Title! - 08 [12345678]");
            var downloadClientRomInfo = Parser.Parser.ParseTitle("[DHD] Game Title! - 08 (1280x720 10bit AAC) [ABCD1234]");
            var localRom = new LocalGameFile
            {
                FileRomInfo = fileRomInfo,
                FolderRomInfo = folderRomInfo,
                DownloadClientRomInfo = downloadClientRomInfo,
                Path = @"C:\Test\Unsorted TV\Game.Title.S01\Game.Title.S01E01.mkv".AsOsAgnostic(),
                Game = _series
            };

            Subject.Aggregate(localRom, null);

            localRom.ReleaseHash.Should().Be("ABCDEFGH");
        }

        [Test]
        public void should_fallback_to_downloadclient()
        {
            var fileRomInfo = Parser.Parser.ParseTitle("[DHD] Game Title! - 08 (1280x720 10bit AAC)");
            var downloadClientRomInfo = Parser.Parser.ParseTitle("[DHD] Game Title! - 08 (1280x720 10bit AAC) [ABCD1234]");
            var folderRomInfo = Parser.Parser.ParseTitle("[DHD] Game Title! - 08 [12345678]");
            var localRom = new LocalGameFile
            {
                FileRomInfo = fileRomInfo,
                FolderRomInfo = folderRomInfo,
                DownloadClientRomInfo = downloadClientRomInfo,
                Path = @"C:\Test\Unsorted TV\Game.Title.S01\Game.Title.S01E01.WEB-DL.mkv".AsOsAgnostic(),
                Game = _series
            };

            Subject.Aggregate(localRom, null);

            localRom.ReleaseHash.Should().Be("ABCD1234");
        }

        [Test]
        public void should_fallback_to_folder()
        {
            var fileRomInfo = Parser.Parser.ParseTitle("[DHD] Game Title! - 08 (1280x720 10bit AAC)");
            var downloadClientRomInfo = Parser.Parser.ParseTitle("[DHD] Game Title! - 08 (1280x720 10bit AAC)");
            var folderRomInfo = Parser.Parser.ParseTitle("[DHD] Game Title! - 08 [12345678]");
            var localRom = new LocalGameFile
            {
                FileRomInfo = fileRomInfo,
                FolderRomInfo = folderRomInfo,
                DownloadClientRomInfo = downloadClientRomInfo,
                Path = @"C:\Test\Unsorted TV\Game.Title.S01\Game.Title.S01E01.WEB-DL.mkv".AsOsAgnostic(),
                Game = _series
            };

            Subject.Aggregate(localRom, null);

            localRom.ReleaseHash.Should().Be("12345678");
        }
    }
}
