using System.Collections.Generic;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Romarr.Common.Disk;
using Romarr.Core.Download;
using Romarr.Core.Download.TrackedDownloads;
using Romarr.Core.History;
using Romarr.Core.MediaFiles;
using Romarr.Core.MediaFiles.GameFileImport;
using Romarr.Core.Messaging.Events;
using Romarr.Core.Parser;
using Romarr.Core.Parser.Model;
using Romarr.Core.Test.Framework;
using Romarr.Core.Games;
using Romarr.Test.Common;

namespace Romarr.Core.Test.Download.CompletedDownloadServiceTests
{
    [TestFixture]
    public class ImportFixture : CoreTest<CompletedDownloadService>
    {
        private TrackedDownload _trackedDownload;
        private Rom _gameFile1;
        private Rom _gameFile2;
        private Rom _gameFile3;

        [SetUp]
        public void Setup()
        {
            _gameFile1 = new Rom { Id = 1, PlatformNumber = 1, FileNumber = 1 };
            _gameFile2 = new Rom { Id = 2, PlatformNumber = 1, FileNumber = 2 };
            _gameFile3 = new Rom { Id = 2, PlatformNumber = 1, FileNumber = 3 };

            var completed = Builder<DownloadClientItem>.CreateNew()
                                                    .With(h => h.Status = DownloadItemStatus.Completed)
                                                    .With(h => h.OutputPath = new OsPath(@"C:\DropFolder\MyDownload".AsOsAgnostic()))
                                                    .With(h => h.Title = "Drone.S01E01.HDTV")
                                                    .Build();

            var remoteRom = BuildRemoteGameFile();

            _trackedDownload = Builder<TrackedDownload>.CreateNew()
                    .With(c => c.State = TrackedDownloadState.Downloading)
                    .With(c => c.DownloadItem = completed)
                    .With(c => c.RemoteRom = remoteRom)
                    .Build();

            Mocker.GetMock<IDownloadClient>()
              .SetupGet(c => c.Definition)
              .Returns(new DownloadClientDefinition { Id = 1, Name = "testClient" });

            Mocker.GetMock<IProvideDownloadClient>()
                  .Setup(c => c.Get(It.IsAny<int>()))
                  .Returns(Mocker.GetMock<IDownloadClient>().Object);

            Mocker.GetMock<IHistoryService>()
                  .Setup(s => s.MostRecentForDownloadId(_trackedDownload.DownloadItem.DownloadId))
                  .Returns(new FileHistory());

            Mocker.GetMock<IParsingService>()
                  .Setup(s => s.GetGame("Drone.S01E01.HDTV"))
                  .Returns(remoteRom.Game);

            Mocker.GetMock<IHistoryService>()
                  .Setup(s => s.FindByDownloadId(It.IsAny<string>()))
                  .Returns(new List<FileHistory>());

            Mocker.GetMock<IProvideImportItemService>()
                  .Setup(s => s.ProvideImportItem(It.IsAny<DownloadClientItem>(), It.IsAny<DownloadClientItem>()))
                  .Returns<DownloadClientItem, DownloadClientItem>((i, p) => i);

            Mocker.GetMock<IRomService>()
                .Setup(s => s.GetRoms(It.IsAny<IEnumerable<int>>()))
                .Returns(new List<Rom>());
        }

        private RemoteRom BuildRemoteGameFile()
        {
            return new RemoteRom
            {
                Game = new Game(),
                Roms = new List<Rom>
                {
                    _gameFile1
                }
            };
        }

        private void GivenABadlyNamedDownload()
        {
            _trackedDownload.DownloadItem.DownloadId = "1234";
            _trackedDownload.DownloadItem.Title = "Droned Pilot"; // Set a badly named download
            Mocker.GetMock<IHistoryService>()
               .Setup(s => s.MostRecentForDownloadId(It.Is<string>(i => i == "1234")))
               .Returns(new FileHistory() { SourceTitle = "Droned S01E01" });

            Mocker.GetMock<IParsingService>()
               .Setup(s => s.GetGame(It.IsAny<string>()))
               .Returns((Game)null);

            Mocker.GetMock<IParsingService>()
                .Setup(s => s.GetGame("Droned S01E01"))
                .Returns(BuildRemoteGameFile().Game);
        }

        private void GivenSeriesMatch()
        {
            Mocker.GetMock<IParsingService>()
                  .Setup(s => s.GetGame(It.IsAny<string>()))
                  .Returns(_trackedDownload.RemoteRom.Game);
        }

        [Test]
        public void should_not_mark_as_imported_if_all_files_were_rejected()
        {
            Mocker.GetMock<IDownloadedFilesImportService>()
                .Setup(v => v.ProcessPath(It.IsAny<string>(), It.IsAny<ImportMode>(), It.IsAny<Game>(), It.IsAny<DownloadClientItem>()))
                .Returns(new List<ImportResult>
                {
                    new ImportResult(
                        new ImportDecision(
                            new LocalGameFile { Path = @"C:\TestPath\Droned.S01E01.mkv", Roms = { _gameFile1 } },
                            new ImportRejection(ImportRejectionReason.Unknown, "Rejected!")),
                        "Test Failure"),

                    new ImportResult(
                        new ImportDecision(
                            new LocalGameFile { Path = @"C:\TestPath\Droned.S01E02.mkv", Roms = { _gameFile2 } },
                            new ImportRejection(ImportRejectionReason.Unknown, "Rejected!")),
                        "Test Failure")
                });

            Subject.Import(_trackedDownload);

            Mocker.GetMock<IEventAggregator>()
                .Verify(v => v.PublishEvent<DownloadCompletedEvent>(It.IsAny<DownloadCompletedEvent>()), Times.Never());

            AssertNotImported();
        }

        [Test]
        public void should_not_mark_as_imported_if_no_gameFiles_were_parsed()
        {
            Mocker.GetMock<IDownloadedFilesImportService>()
                  .Setup(v => v.ProcessPath(It.IsAny<string>(), It.IsAny<ImportMode>(), It.IsAny<Game>(), It.IsAny<DownloadClientItem>()))
                  .Returns(new List<ImportResult>
                           {
                               new ImportResult(
                                   new ImportDecision(
                                       new LocalGameFile { Path = @"C:\TestPath\Droned.S01E01.mkv", Roms = { _gameFile1 } }, new ImportRejection(ImportRejectionReason.Unknown, "Rejected!")),
                                   "Test Failure"),

                               new ImportResult(
                                   new ImportDecision(
                                       new LocalGameFile { Path = @"C:\TestPath\Droned.S01E02.mkv", Roms = { _gameFile2 } }, new ImportRejection(ImportRejectionReason.Unknown, "Rejected!")),
                                   "Test Failure")
                           });

            _trackedDownload.RemoteRom.Roms.Clear();

            Subject.Import(_trackedDownload);

            AssertNotImported();
        }

        [Test]
        public void should_not_mark_as_imported_if_all_files_were_skipped()
        {
            Mocker.GetMock<IDownloadedFilesImportService>()
                  .Setup(v => v.ProcessPath(It.IsAny<string>(), It.IsAny<ImportMode>(), It.IsAny<Game>(), It.IsAny<DownloadClientItem>()))
                  .Returns(new List<ImportResult>
                           {
                               new ImportResult(new ImportDecision(new LocalGameFile { Path = @"C:\TestPath\Droned.S01E01.mkv", Roms = { _gameFile1 } }), "Test Failure"),
                               new ImportResult(new ImportDecision(new LocalGameFile { Path = @"C:\TestPath\Droned.S01E02.mkv", Roms = { _gameFile2 } }), "Test Failure")
                           });

            Subject.Import(_trackedDownload);

            AssertNotImported();
        }

        [Test]
        public void should_not_mark_as_imported_if_some_of_gameFiles_were_not_imported()
        {
            _trackedDownload.RemoteRom.Roms = new List<Rom>
            {
                new Rom(),
                new Rom(),
                new Rom()
            };

            Mocker.GetMock<IDownloadedFilesImportService>()
                  .Setup(v => v.ProcessPath(It.IsAny<string>(), It.IsAny<ImportMode>(), It.IsAny<Game>(), It.IsAny<DownloadClientItem>()))
                  .Returns(new List<ImportResult>
                           {
                               new ImportResult(new ImportDecision(new LocalGameFile { Path = @"C:\TestPath\Droned.S01E01.mkv" })),
                               new ImportResult(new ImportDecision(new LocalGameFile { Path = @"C:\TestPath\Droned.S01E01.mkv" }), "Test Failure"),
                               new ImportResult(new ImportDecision(new LocalGameFile { Path = @"C:\TestPath\Droned.S01E01.mkv" }), "Test Failure")
                           });

            Mocker.GetMock<IHistoryService>()
                  .Setup(s => s.FindByDownloadId(It.IsAny<string>()))
                  .Returns(new List<FileHistory>());

            Subject.Import(_trackedDownload);

            AssertNotImported();
        }

        [Test]
        public void should_not_mark_as_imported_if_some_of_gameFiles_were_not_imported_including_history()
        {
            _trackedDownload.RemoteRom.Roms = new List<Rom>
                                                      {
                                                          new Rom(),
                                                          new Rom(),
                                                          new Rom()
                                                      };

            Mocker.GetMock<IDownloadedFilesImportService>()
                  .Setup(v => v.ProcessPath(It.IsAny<string>(), It.IsAny<ImportMode>(), It.IsAny<Game>(), It.IsAny<DownloadClientItem>()))
                  .Returns(new List<ImportResult>
                           {
                               new ImportResult(new ImportDecision(new LocalGameFile { Path = @"C:\TestPath\Droned.S01E01.mkv" })),
                               new ImportResult(new ImportDecision(new LocalGameFile { Path = @"C:\TestPath\Droned.S01E01.mkv" }), "Test Failure"),
                               new ImportResult(new ImportDecision(new LocalGameFile { Path = @"C:\TestPath\Droned.S01E01.mkv" }), "Test Failure")
                           });

            var history = Builder<FileHistory>.CreateListOfSize(2)
                                                  .BuildList();

            Mocker.GetMock<IHistoryService>()
                  .Setup(s => s.FindByDownloadId(It.IsAny<string>()))
                  .Returns(history);

            Mocker.GetMock<ITrackedDownloadAlreadyImported>()
                  .Setup(s => s.IsImported(_trackedDownload, history))
                  .Returns(true);

            Subject.Import(_trackedDownload);

            AssertNotImported();
        }

        [Test]
        public void should_mark_as_imported_if_all_gameFiles_were_imported()
        {
            var gameFile1 = new Rom { Id = 1 };
            var gameFile2 = new Rom { Id = 2 };
            _trackedDownload.RemoteRom.Roms = new List<Rom> { gameFile1, gameFile2 };

            Mocker.GetMock<IDownloadedFilesImportService>()
                  .Setup(v => v.ProcessPath(It.IsAny<string>(), It.IsAny<ImportMode>(), It.IsAny<Game>(), It.IsAny<DownloadClientItem>()))
                  .Returns(new List<ImportResult>
                           {
                               new ImportResult(
                                   new ImportDecision(
                                       new LocalGameFile { Path = @"C:\TestPath\Droned.S01E01.mkv", Roms = new List<Rom> { gameFile1 } })),

                               new ImportResult(
                                   new ImportDecision(
                                       new LocalGameFile { Path = @"C:\TestPath\Droned.S01E02.mkv", Roms = new List<Rom> { gameFile2 } }))
                           });

            Subject.Import(_trackedDownload);

            AssertImported();
        }

        [Test]
        public void should_mark_as_imported_if_all_gameFiles_were_imported_including_history()
        {
            var gameFile1 = new Rom { Id = 1 };
            var gameFile2 = new Rom { Id = 2 };
            _trackedDownload.RemoteRom.Roms = new List<Rom> { gameFile1, gameFile2 };

            Mocker.GetMock<IDownloadedFilesImportService>()
                .Setup(v => v.ProcessPath(It.IsAny<string>(), It.IsAny<ImportMode>(), It.IsAny<Game>(), It.IsAny<DownloadClientItem>()))
                .Returns(
                    new List<ImportResult>
                    {
                        new ImportResult(
                            new ImportDecision(
                                new LocalGameFile
                                {
                                    Path = @"C:\TestPath\Droned.S01E01.mkv", Roms = new List<Rom> { gameFile1 }
                                })),

                        new ImportResult(
                            new ImportDecision(
                                new LocalGameFile
                                {
                                    Path = @"C:\TestPath\Droned.S01E02.mkv", Roms = new List<Rom> { gameFile2 }
                                }),
                            "Test Failure")
                    });

            var history = Builder<FileHistory>.CreateListOfSize(2)
                                                  .BuildList();

            Mocker.GetMock<IHistoryService>()
                  .Setup(s => s.FindByDownloadId(It.IsAny<string>()))
                  .Returns(history);

            Mocker.GetMock<ITrackedDownloadAlreadyImported>()
                  .Setup(s => s.IsImported(It.IsAny<TrackedDownload>(), It.IsAny<List<FileHistory>>()))
                  .Returns(true);

            Subject.Import(_trackedDownload);

            AssertImported();
        }

        [Test]
        public void should_mark_as_imported_if_double_gameFile_file_is_imported()
        {
            var gameFile1 = new Rom { Id = 1 };
            var gameFile2 = new Rom { Id = 2 };
            _trackedDownload.RemoteRom.Roms = new List<Rom> { gameFile1, gameFile2 };

            Mocker.GetMock<IDownloadedFilesImportService>()
                  .Setup(v => v.ProcessPath(It.IsAny<string>(), It.IsAny<ImportMode>(), It.IsAny<Game>(), It.IsAny<DownloadClientItem>()))
                  .Returns(new List<ImportResult>
                           {
                               new ImportResult(
                                   new ImportDecision(
                                       new LocalGameFile { Path = @"C:\TestPath\Droned.S01E01-E02.mkv", Roms = new List<Rom> { gameFile1, gameFile2 } }))
                           });

            Subject.Import(_trackedDownload);

            AssertImported();
        }

        [Test]
        public void should_mark_as_imported_if_all_gameFiles_were_imported_but_extra_files_were_not()
        {
            GivenSeriesMatch();

            _trackedDownload.RemoteRom.Roms = new List<Rom>
                                                      {
                                                          new Rom()
                                                      };

            Mocker.GetMock<IDownloadedFilesImportService>()
                  .Setup(v => v.ProcessPath(It.IsAny<string>(), It.IsAny<ImportMode>(), It.IsAny<Game>(), It.IsAny<DownloadClientItem>()))
                  .Returns(new List<ImportResult>
                           {
                               new ImportResult(new ImportDecision(new LocalGameFile { Path = @"C:\TestPath\Droned.S01E01.mkv", Roms = _trackedDownload.RemoteRom.Roms })),
                               new ImportResult(new ImportDecision(new LocalGameFile { Path = @"C:\TestPath\Droned.S01E01.mkv" }), "Test Failure")
                           });

            Subject.Import(_trackedDownload);

            AssertImported();
        }

        [Test]
        public void should_mark_as_imported_if_the_download_can_be_tracked_using_the_source_seriesid()
        {
            GivenABadlyNamedDownload();

            Mocker.GetMock<IDownloadedFilesImportService>()
                  .Setup(v => v.ProcessPath(It.IsAny<string>(), It.IsAny<ImportMode>(), It.IsAny<Game>(), It.IsAny<DownloadClientItem>()))
                  .Returns(new List<ImportResult>
                           {
                               new ImportResult(new ImportDecision(new LocalGameFile { Path = @"C:\TestPath\Droned.S01E01.mkv", Roms = _trackedDownload.RemoteRom.Roms }))
                           });

            Mocker.GetMock<IGameService>()
                  .Setup(v => v.GetGame(It.IsAny<int>()))
                  .Returns(BuildRemoteGameFile().Game);

            Subject.Import(_trackedDownload);

            AssertImported();
        }

        private void AssertNotImported()
        {
            Mocker.GetMock<IEventAggregator>()
                  .Verify(v => v.PublishEvent(It.IsAny<DownloadCompletedEvent>()), Times.Never());

            _trackedDownload.State.Should().Be(TrackedDownloadState.ImportBlocked);
        }

        private void AssertImported()
        {
            Mocker.GetMock<IDownloadedFilesImportService>()
                .Verify(v => v.ProcessPath(_trackedDownload.DownloadItem.OutputPath.FullPath, ImportMode.Auto, _trackedDownload.RemoteRom.Game, _trackedDownload.DownloadItem), Times.Once());

            Mocker.GetMock<IEventAggregator>()
                  .Verify(v => v.PublishEvent(It.IsAny<DownloadCompletedEvent>()), Times.Once());

            _trackedDownload.State.Should().Be(TrackedDownloadState.Imported);
        }
    }
}
