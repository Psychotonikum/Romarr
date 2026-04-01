using System;
using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using Romarr.Core.Download;
using Romarr.Core.Download.TrackedDownloads;
using Romarr.Core.Parser.Model;
using Romarr.Core.Queue;
using Romarr.Core.Test.Framework;
using Romarr.Core.Games;

namespace Romarr.Core.Test.QueueTests
{
    [TestFixture]
    public class ObsoleteQueueServiceFixture : CoreTest<ObsoleteQueueService>
    {
        private List<TrackedDownload> _trackedDownloads;

        [SetUp]
        public void SetUp()
        {
            var downloadClientInfo = Builder<DownloadClientItemClientInfo>.CreateNew().Build();

            var downloadItem = Builder<Romarr.Core.Download.DownloadClientItem>.CreateNew()
                                        .With(v => v.RemainingTime = TimeSpan.FromSeconds(10))
                                        .With(v => v.DownloadClientInfo = downloadClientInfo)
                                        .Build();

            var game = Builder<Game>.CreateNew()
                                        .Build();

            var roms = Builder<Rom>.CreateListOfSize(3)
                                          .All()
                                          .With(e => e.GameId = game.Id)
                                          .Build();

            var remoteRom = Builder<RemoteRom>.CreateNew()
                                                   .With(r => r.Game = game)
                                                   .With(r => r.Roms = new List<Rom>(roms))
                                                   .With(r => r.ParsedRomInfo = new ParsedRomInfo())
                                                   .Build();

            _trackedDownloads = Builder<TrackedDownload>.CreateListOfSize(1)
                .All()
                .With(v => v.IsTrackable = true)
                .With(v => v.DownloadItem = downloadItem)
                .With(v => v.RemoteRom = remoteRom)
                .Build()
                .ToList();
        }

        [Test]
        public void queue_items_should_have_id()
        {
            Subject.Handle(new TrackedDownloadRefreshedEvent(_trackedDownloads));

            var queue = Subject.GetQueue();

            queue.Should().HaveCount(3);

            queue.All(v => v.Id > 0).Should().BeTrue();

            var distinct = queue.Select(v => v.Id).Distinct().ToArray();

            distinct.Should().HaveCount(3);
        }
    }
}
