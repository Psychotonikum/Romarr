using System;
using System.Collections.Generic;
using System.IO;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using Romarr.Common.Disk;
using Romarr.Core.Configuration;
using Romarr.Core.DecisionEngine;
using Romarr.Core.DecisionEngine.Specifications.RssSync;
using Romarr.Core.IndexerSearch.Definitions;
using Romarr.Core.MediaFiles;
using Romarr.Core.Parser.Model;
using Romarr.Core.Profiles.Qualities;
using Romarr.Core.Qualities;
using Romarr.Core.Test.Framework;
using Romarr.Core.Games;
using Romarr.Test.Common;

namespace Romarr.Core.Test.DecisionEngineTests.RssSync
{
    [TestFixture]
    public class DeletedRomFileSpecificationFixture : CoreTest<DeletedRomFileSpecification>
    {
        private RemoteRom _parseResultMulti;
        private RemoteRom _parseResultSingle;
        private RomFile _firstFile;
        private RomFile _secondFile;

        [SetUp]
        public void Setup()
        {
            _firstFile = new RomFile
            {
                Id = 1,
                RelativePath = "My.Game.S01E01.mkv",
                Quality = new QualityModel(Quality.Bluray1080p, new Revision(version: 1)),
                DateAdded = DateTime.Now
            };
            _secondFile = new RomFile
            {
                Id = 2,
                RelativePath = "My.Game.S01E02.mkv",
                Quality = new QualityModel(Quality.Bluray1080p, new Revision(version: 1)),
                DateAdded = DateTime.Now
            };

            var singleGameFileList = new List<Rom> { new Rom { RomFile = _firstFile, RomFileId = 1 } };
            var doubleGameFileList = new List<Rom>
            {
                new Rom { RomFile = _firstFile, RomFileId = 1 },
                new Rom { RomFile = _secondFile, RomFileId = 2 }
            };

            var fakeSeries = Builder<Game>.CreateNew()
                         .With(c => c.QualityProfile = new QualityProfile { Cutoff = Quality.Bluray1080p.Id })
                         .With(c => c.Path = @"C:\Game\My.Game".AsOsAgnostic())
                         .Build();

            _parseResultMulti = new RemoteRom
            {
                Game = fakeSeries,
                ParsedRomInfo = new ParsedRomInfo { Quality = new QualityModel(Quality.DVD, new Revision(version: 2)) },
                Roms = doubleGameFileList
            };

            _parseResultSingle = new RemoteRom
            {
                Game = fakeSeries,
                ParsedRomInfo = new ParsedRomInfo { Quality = new QualityModel(Quality.DVD, new Revision(version: 2)) },
                Roms = singleGameFileList
            };

            GivenUnmonitorDeletedGameFiles(true);
        }

        private void GivenUnmonitorDeletedGameFiles(bool enabled)
        {
            Mocker.GetMock<IConfigService>()
                  .SetupGet(v => v.AutoUnmonitorPreviouslyDownloadedGameFiles)
                  .Returns(enabled);
        }

        private void WithExistingFile(RomFile romFile)
        {
            var path = Path.Combine(@"C:\Game\My.Game".AsOsAgnostic(), romFile.RelativePath);

            Mocker.GetMock<IDiskProvider>()
                  .Setup(v => v.FileExists(path))
                  .Returns(true);
        }

        [Test]
        public void should_return_true_when_unmonitor_deleted_episdes_is_off()
        {
            GivenUnmonitorDeletedGameFiles(false);

            Subject.IsSatisfiedBy(_parseResultSingle, new()).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_true_when_searching()
        {
            Subject.IsSatisfiedBy(_parseResultSingle, new ReleaseDecisionInformation(false, new PlatformSearchCriteria())).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_true_if_file_exists()
        {
            WithExistingFile(_firstFile);

            Subject.IsSatisfiedBy(_parseResultSingle, new()).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_false_if_file_is_missing()
        {
            Subject.IsSatisfiedBy(_parseResultSingle, new()).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_true_if_both_of_multiple_gameFile_exist()
        {
            WithExistingFile(_firstFile);
            WithExistingFile(_secondFile);

            Subject.IsSatisfiedBy(_parseResultMulti, new()).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_false_if_one_of_multiple_gameFile_is_missing()
        {
            WithExistingFile(_firstFile);

            Subject.IsSatisfiedBy(_parseResultMulti, new()).Accepted.Should().BeFalse();
        }
    }
}
