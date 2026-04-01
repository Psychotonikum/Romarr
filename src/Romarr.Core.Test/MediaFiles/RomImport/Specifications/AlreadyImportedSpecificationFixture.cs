using System;
using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Romarr.Core.Download;
using Romarr.Core.History;
using Romarr.Core.MediaFiles.GameFileImport.Specifications;
using Romarr.Core.Parser.Model;
using Romarr.Core.Test.Framework;
using Romarr.Core.Games;
using Romarr.Test.Common;

namespace Romarr.Core.Test.MediaFiles.GameFileImport.Specifications
{
    [TestFixture]
    public class AlreadyImportedSpecificationFixture : CoreTest<AlreadyImportedSpecification>
    {
        private Game _series;
        private Rom _gameFile;
        private LocalGameFile _localRom;
        private DownloadClientItem _downloadClientItem;

        [SetUp]
        public void Setup()
        {
            _series = Builder<Game>.CreateNew()
                                     .With(s => s.GameType = GameTypes.Standard)
                                     .With(s => s.Path = @"C:\Test\TV\30 Rock".AsOsAgnostic())
                                     .Build();

            _gameFile = Builder<Rom>.CreateNew()
                .With(e => e.PlatformNumber = 1)
                .With(e => e.AirDateUtc = DateTime.UtcNow)
                .Build();

            _localRom = new LocalGameFile
                                {
                                    Path = @"C:\Test\Unsorted\30 Rock\30.rock.s01e01.avi".AsOsAgnostic(),
                                    Roms = new List<Rom> { _gameFile },
                                    Game = _series
                                };

            _downloadClientItem = Builder<DownloadClientItem>.CreateNew()
                .Build();
        }

        private void GivenHistory(List<FileHistory> history)
        {
            Mocker.GetMock<IHistoryService>()
                .Setup(s => s.FindByRomId(It.IsAny<int>()))
                .Returns(history);
        }

        [Test]
        public void should_accepted_if_download_client_item_is_null()
        {
            Subject.IsSatisfiedBy(_localRom, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_accept_if_gameFile_does_not_have_file()
        {
            _gameFile.RomFileId = 0;

            Subject.IsSatisfiedBy(_localRom, _downloadClientItem).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_accept_if_gameFile_has_not_been_imported()
        {
            var history = Builder<FileHistory>.CreateListOfSize(1)
                .All()
                .With(h => h.FileId = _gameFile.Id)
                .With(h => h.EventType = FileHistoryEventType.Grabbed)
                .Build()
                .ToList();

            GivenHistory(history);

            Subject.IsSatisfiedBy(_localRom, _downloadClientItem).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_accept_if_gameFile_was_grabbed_after_being_imported()
        {
            var history = Builder<FileHistory>.CreateListOfSize(3)
                .All()
                .With(h => h.FileId = _gameFile.Id)
                .TheFirst(1)
                .With(h => h.EventType = FileHistoryEventType.Grabbed)
                .With(h => h.Date = DateTime.UtcNow)
                .TheNext(1)
                .With(h => h.EventType = FileHistoryEventType.DownloadFolderImported)
                .With(h => h.Date = DateTime.UtcNow.AddDays(-1))
                .TheNext(1)
                .With(h => h.EventType = FileHistoryEventType.Grabbed)
                .With(h => h.Date = DateTime.UtcNow.AddDays(-2))
                .Build()
                .ToList();

            GivenHistory(history);

            Subject.IsSatisfiedBy(_localRom, _downloadClientItem).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_reject_if_gameFile_imported_after_being_grabbed()
        {
            var history = Builder<FileHistory>.CreateListOfSize(2)
                .All()
                .With(h => h.FileId = _gameFile.Id)
                .TheFirst(1)
                .With(h => h.EventType = FileHistoryEventType.DownloadFolderImported)
                .With(h => h.Date = DateTime.UtcNow.AddDays(-1))
                .TheNext(1)
                .With(h => h.EventType = FileHistoryEventType.Grabbed)
                .With(h => h.Date = DateTime.UtcNow.AddDays(-2))
                .Build()
                .ToList();

            GivenHistory(history);

            Subject.IsSatisfiedBy(_localRom, _downloadClientItem).Accepted.Should().BeFalse();
        }
    }
}
