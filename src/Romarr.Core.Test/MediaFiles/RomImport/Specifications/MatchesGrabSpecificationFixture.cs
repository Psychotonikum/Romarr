using System.Collections.Generic;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using Romarr.Common.Extensions;
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
    public class MatchesGrabSpecificationFixture : CoreTest<MatchesGrabSpecification>
    {
        private Rom _gameFile1;
        private Rom _gameFile2;
        private Rom _gameFile3;
        private LocalGameFile _localRom;
        private DownloadClientItem _downloadClientItem;

        [SetUp]
        public void Setup()
        {
            _gameFile1 = Builder<Rom>.CreateNew()
                .With(e => e.Id = 1)
                .Build();

            _gameFile2 = Builder<Rom>.CreateNew()
                .With(e => e.Id = 2)
                .Build();

            _gameFile3 = Builder<Rom>.CreateNew()
                .With(e => e.Id = 3)
                .Build();

            _localRom = Builder<LocalGameFile>.CreateNew()
                                                 .With(l => l.Path = @"C:\Test\Unsorted\Game.Title.S01E01.720p.HDTV-Romarr\S01E05.mkv".AsOsAgnostic())
                                                 .With(l => l.Roms = new List<Rom> { _gameFile1 })
                                                 .With(l => l.Release = null)
                                                 .Build();

            _downloadClientItem = Builder<DownloadClientItem>.CreateNew().Build();
        }

        private void GivenHistoryForGameFiles(params Rom[] roms)
        {
            if (roms.Empty())
            {
                return;
            }

            var grabbedHistories = Builder<FileHistory>.CreateListOfSize(roms.Length)
                .All()
                .With(h => h.EventType == FileHistoryEventType.Grabbed)
                .BuildList();

            for (var i = 0; i < grabbedHistories.Count; i++)
            {
                grabbedHistories[i].FileId = roms[i].Id;
            }

            _localRom.Release = new GrabbedReleaseInfo(grabbedHistories);
        }

        [Test]
        public void should_be_accepted_for_existing_file()
        {
            _localRom.ExistingFile = true;

            Subject.IsSatisfiedBy(_localRom, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_be_accepted_if_no_download_client_item()
        {
            Subject.IsSatisfiedBy(_localRom, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_be_accepted_if_no_grabbed_release_info()
        {
            GivenHistoryForGameFiles();

            Subject.IsSatisfiedBy(_localRom, _downloadClientItem).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_be_accepted_if_file_gameFile_matches_single_grabbed_release_info()
        {
            GivenHistoryForGameFiles(_gameFile1);

            Subject.IsSatisfiedBy(_localRom, _downloadClientItem).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_be_accepted_if_file_gameFile_is_in_multi_gameFile_grabbed_release_info()
        {
            GivenHistoryForGameFiles(_gameFile1, _gameFile2);

            Subject.IsSatisfiedBy(_localRom, _downloadClientItem).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_be_rejected_if_file_gameFile_does_not_match_single_grabbed_release_info()
        {
            GivenHistoryForGameFiles(_gameFile2);

            Subject.IsSatisfiedBy(_localRom, _downloadClientItem).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_be_rejected_if_file_gameFile_is_not_in_multi_gameFile_grabbed_release_info()
        {
            GivenHistoryForGameFiles(_gameFile2, _gameFile3);

            Subject.IsSatisfiedBy(_localRom, _downloadClientItem).Accepted.Should().BeFalse();
        }
    }
}
