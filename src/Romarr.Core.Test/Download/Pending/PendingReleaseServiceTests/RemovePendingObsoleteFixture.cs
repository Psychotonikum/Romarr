using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using Romarr.Common.Crypto;
using Romarr.Core.Download.Pending;
using Romarr.Core.Lifecycle;
using Romarr.Core.Parser;
using Romarr.Core.Parser.Model;
using Romarr.Core.Test.Framework;
using Romarr.Core.Games;

namespace Romarr.Core.Test.Download.Pending.PendingReleaseServiceTests
{
    [TestFixture]
    [NonParallelizable]
    public class RemovePendingObsoleteFixture : CoreTest<PendingReleaseService>
    {
        private List<PendingRelease> _pending;
        private Rom _gameFile;

        [SetUp]
        public void Setup()
        {
            _pending = new List<PendingRelease>();

            _gameFile = Builder<Rom>.CreateNew()
                                       .Build();

            Mocker.GetMock<IPendingReleaseRepository>()
                 .Setup(s => s.AllByGameId(It.IsAny<int>()))
                 .Returns(_pending);

            Mocker.GetMock<IPendingReleaseRepository>()
                  .Setup(s => s.All())
                  .Returns(_pending);

            Mocker.GetMock<IGameService>()
                  .Setup(s => s.GetGame(It.IsAny<int>()))
                  .Returns(new Game());

            Mocker.GetMock<IGameService>()
                  .Setup(s => s.GetGame(It.IsAny<IEnumerable<int>>()))
                  .Returns(new List<Game> { new Game() });

            Mocker.GetMock<IParsingService>()
                  .Setup(s => s.Map(It.IsAny<ParsedRomInfo>(), It.IsAny<Game>()))
                  .Returns(new RemoteRom { Roms = new List<Rom> { _gameFile } });

            Mocker.GetMock<IParsingService>()
                  .Setup(s => s.GetRoms(It.IsAny<ParsedRomInfo>(), It.IsAny<Game>(), It.IsAny<bool>(), null))
                  .Returns(new List<Rom> { _gameFile });
        }

        private void AddPending(int id, int platformNumber, int[] roms)
        {
            _pending.Add(new PendingRelease
             {
                 Id = id,
                 Title = "Game.Title.S01E05.abc-Romarr",
                 ParsedRomInfo = new ParsedRomInfo { PlatformNumber = platformNumber, RomNumbers = roms },
                 Release = Builder<ReleaseInfo>.CreateNew().Build()
             });
        }

        private void InitializeReleases()
        {
            Subject.Handle(new ApplicationStartedEvent());
        }

        [Test]
        public void should_remove_same_release()
        {
            AddPending(id: 1, platformNumber: 2, roms: new[] { 3 });

            var queueId = HashConverter.GetHashInt31(string.Format("pending-{0}-ep{1}", 1, _gameFile.Id));

            InitializeReleases();
            Subject.RemovePendingQueueItemsObsolete(queueId);

            AssertRemoved(1);
        }

        [Test]
        public void should_remove_multiple_releases_release()
        {
            AddPending(id: 1, platformNumber: 2, roms: new[] { 1 });
            AddPending(id: 2, platformNumber: 2, roms: new[] { 2 });
            AddPending(id: 3, platformNumber: 2, roms: new[] { 3 });
            AddPending(id: 4, platformNumber: 2, roms: new[] { 3 });

            var queueId = HashConverter.GetHashInt31(string.Format("pending-{0}-ep{1}", 3, _gameFile.Id));

            InitializeReleases();
            Subject.RemovePendingQueueItemsObsolete(queueId);

            AssertRemoved(3, 4);
        }

        [Test]
        public void should_not_remove_different_platform()
        {
            AddPending(id: 1, platformNumber: 2, roms: new[] { 1 });
            AddPending(id: 2, platformNumber: 2, roms: new[] { 1 });
            AddPending(id: 3, platformNumber: 3, roms: new[] { 1 });
            AddPending(id: 4, platformNumber: 3, roms: new[] { 1 });

            var queueId = HashConverter.GetHashInt31(string.Format("pending-{0}-ep{1}", 1, _gameFile.Id));

            InitializeReleases();
            Subject.RemovePendingQueueItemsObsolete(queueId);

            AssertRemoved(1, 2);
        }

        [Test]
        public void should_not_remove_different_gameFiles()
        {
            AddPending(id: 1, platformNumber: 2, roms: new[] { 1 });
            AddPending(id: 2, platformNumber: 2, roms: new[] { 1 });
            AddPending(id: 3, platformNumber: 2, roms: new[] { 2 });
            AddPending(id: 4, platformNumber: 2, roms: new[] { 3 });

            var queueId = HashConverter.GetHashInt31(string.Format("pending-{0}-ep{1}", 1, _gameFile.Id));

            InitializeReleases();
            Subject.RemovePendingQueueItemsObsolete(queueId);

            AssertRemoved(1, 2);
        }

        [Test]
        public void should_not_remove_multigameFiles()
        {
            AddPending(id: 1, platformNumber: 2, roms: new[] { 1 });
            AddPending(id: 2, platformNumber: 2, roms: new[] { 1, 2 });

            var queueId = HashConverter.GetHashInt31(string.Format("pending-{0}-ep{1}", 1, _gameFile.Id));

            InitializeReleases();
            Subject.RemovePendingQueueItemsObsolete(queueId);

            AssertRemoved(1);
        }

        [Test]
        public void should_not_remove_singlegameFiles()
        {
            AddPending(id: 1, platformNumber: 2, roms: new[] { 1 });
            AddPending(id: 2, platformNumber: 2, roms: new[] { 1, 2 });

            var queueId = HashConverter.GetHashInt31(string.Format("pending-{0}-ep{1}", 2, _gameFile.Id));

            InitializeReleases();
            Subject.RemovePendingQueueItemsObsolete(queueId);

            AssertRemoved(2);
        }

        private void AssertRemoved(params int[] ids)
        {
            Mocker.GetMock<IPendingReleaseRepository>().Verify(c => c.DeleteMany(It.Is<IEnumerable<int>>(s => s.SequenceEqual(ids))));
        }
    }
}
