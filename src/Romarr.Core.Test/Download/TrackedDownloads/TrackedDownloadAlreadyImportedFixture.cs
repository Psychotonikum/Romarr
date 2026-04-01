using System.Collections.Generic;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using Romarr.Core.Download;
using Romarr.Core.Download.TrackedDownloads;
using Romarr.Core.History;
using Romarr.Core.Parser.Model;
using Romarr.Core.Test.Framework;
using Romarr.Core.Games;

namespace Romarr.Core.Test.Download.TrackedDownloads
{
    [TestFixture]
    public class TrackedDownloadAlreadyImportedFixture : CoreTest<TrackedDownloadAlreadyImported>
    {
        private List<Rom> _gameFiles;
        private TrackedDownload _trackedDownload;
        private List<FileHistory> _historyItems;

        [SetUp]
        public void Setup()
        {
            _gameFiles = new List<Rom>();

            var remoteRom = Builder<RemoteRom>.CreateNew()
                                                      .With(r => r.Roms = _gameFiles)
                                                      .Build();

            var downloadItem = Builder<DownloadClientItem>.CreateNew()
                                                         .Build();

            _trackedDownload = Builder<TrackedDownload>.CreateNew()
                                                       .With(t => t.RemoteRom = remoteRom)
                                                       .With(t => t.DownloadItem = downloadItem)
                                                       .Build();

            _historyItems = new List<FileHistory>();
        }

        public void GivenGameFiles(int count)
        {
            _gameFiles.AddRange(Builder<Rom>.CreateListOfSize(count)
                                               .BuildList());
        }

        public void GivenHistoryForGameFile(Rom rom, params FileHistoryEventType[] eventTypes)
        {
            foreach (var eventType in eventTypes)
            {
                _historyItems.Add(
                    Builder<FileHistory>.CreateNew()
                                            .With(h => h.FileId = rom.Id)
                                            .With(h => h.EventType = eventType)
                                            .Build());
            }
        }

        [Test]
        public void should_return_false_if_there_is_no_history()
        {
            GivenGameFiles(1);

            Subject.IsImported(_trackedDownload, _historyItems)
                   .Should()
                   .BeFalse();
        }

        [Test]
        public void should_return_false_if_single_gameFile_download_is_not_imported()
        {
            GivenGameFiles(1);

            GivenHistoryForGameFile(_gameFiles[0], FileHistoryEventType.Grabbed);

            Subject.IsImported(_trackedDownload, _historyItems)
                   .Should()
                   .BeFalse();
        }

        [Test]
        public void should_return_false_if_no_gameFile_in_multi_gameFile_download_is_imported()
        {
            GivenGameFiles(2);

            GivenHistoryForGameFile(_gameFiles[0], FileHistoryEventType.Grabbed);
            GivenHistoryForGameFile(_gameFiles[1], FileHistoryEventType.Grabbed);

            Subject.IsImported(_trackedDownload, _historyItems)
                   .Should()
                   .BeFalse();
        }

        [Test]
        public void should_should_return_false_if_only_one_gameFile_in_multi_gameFile_download_is_imported()
        {
            GivenGameFiles(2);

            GivenHistoryForGameFile(_gameFiles[0], FileHistoryEventType.DownloadFolderImported, FileHistoryEventType.Grabbed);
            GivenHistoryForGameFile(_gameFiles[1], FileHistoryEventType.Grabbed);

            Subject.IsImported(_trackedDownload, _historyItems)
                   .Should()
                   .BeFalse();
        }

        [Test]
        public void should_return_true_if_single_gameFile_download_is_imported()
        {
            GivenGameFiles(1);

            GivenHistoryForGameFile(_gameFiles[0], FileHistoryEventType.DownloadFolderImported, FileHistoryEventType.Grabbed);

            Subject.IsImported(_trackedDownload, _historyItems)
                   .Should()
                   .BeTrue();
        }

        [Test]
        public void should_return_true_if_multi_gameFile_download_is_imported()
        {
            GivenGameFiles(2);

            GivenHistoryForGameFile(_gameFiles[0], FileHistoryEventType.DownloadFolderImported, FileHistoryEventType.Grabbed);
            GivenHistoryForGameFile(_gameFiles[1], FileHistoryEventType.DownloadFolderImported, FileHistoryEventType.Grabbed);

            Subject.IsImported(_trackedDownload, _historyItems)
                   .Should()
                   .BeTrue();
        }
    }
}
