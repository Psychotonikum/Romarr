using System;
using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using Romarr.Core.Blocklisting;
using Romarr.Core.Download;
using Romarr.Core.Qualities;
using Romarr.Core.Test.Framework;

namespace Romarr.Core.Test.Blocklisting
{
    [TestFixture]
    public class BlocklistServiceFixture : CoreTest<BlocklistService>
    {
        private DownloadFailedEvent _event;

        [SetUp]
        public void Setup()
        {
            _event = new DownloadFailedEvent
                     {
                         GameId = 12345,
                         RomIds = new List<int> { 1 },
                         Quality = new QualityModel(Quality.Bluray720p),
                         SourceTitle = "game.title.s01e01",
                         DownloadClient = "SabnzbdClient",
                         DownloadId = "Sabnzbd_nzo_2dfh73k"
                     };

            _event.Data.Add("publishedDate", DateTime.UtcNow.ToString("s") + "Z");
            _event.Data.Add("size", "1000");
            _event.Data.Add("indexer", "nzbs.org");
            _event.Data.Add("protocol", "1");
            _event.Data.Add("message", "Marked as failed");
        }

        [Test]
        public void should_add_to_repository()
        {
            Subject.Handle(_event);

            Mocker.GetMock<IBlocklistRepository>()
                .Verify(v => v.Insert(It.Is<Blocklist>(b => b.RomIds == _event.RomIds)), Times.Once());
        }

        [Test]
        public void should_add_to_repository_missing_size_and_protocol()
        {
            Subject.Handle(_event);

            _event.Data.Remove("size");
            _event.Data.Remove("protocol");

            Mocker.GetMock<IBlocklistRepository>()
                .Verify(v => v.Insert(It.Is<Blocklist>(b => b.RomIds == _event.RomIds)), Times.Once());
        }
    }
}
