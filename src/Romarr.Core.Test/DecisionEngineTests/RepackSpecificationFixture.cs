using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using Romarr.Core.Configuration;
using Romarr.Core.DecisionEngine.Specifications;
using Romarr.Core.MediaFiles;
using Romarr.Core.Parser.Model;
using Romarr.Core.Qualities;
using Romarr.Core.Test.Framework;
using Romarr.Core.Games;

namespace Romarr.Core.Test.DecisionEngineTests
{
    [TestFixture]
    public class RepackSpecificationFixture : CoreTest<RepackSpecification>
    {
        private ParsedRomInfo _parsedRomInfo;
        private List<Rom> _gameFiles;

        [SetUp]
        public void Setup()
        {
            Mocker.Resolve<UpgradableSpecification>();

            _parsedRomInfo = Builder<ParsedRomInfo>.CreateNew()
                                                           .With(p => p.Quality = new QualityModel(Quality.SDTV,
                                                               new Revision(2, 0, false)))
                                                           .With(p => p.ReleaseGroup = "Romarr")
                                                           .Build();

            _gameFiles = Builder<Rom>.CreateListOfSize(1)
                                        .All()
                                        .With(e => e.RomFileId = 0)
                                        .BuildList();
        }

        [Test]
        public void should_return_true_if_it_is_not_a_repack()
        {
            var remoteRom = Builder<RemoteRom>.CreateNew()
                                                      .With(e => e.ParsedRomInfo = _parsedRomInfo)
                                                      .With(e => e.Roms = _gameFiles)
                                                      .Build();

            Subject.IsSatisfiedBy(remoteRom, null)
                   .Accepted
                   .Should()
                   .BeTrue();
        }

        [Test]
        public void should_return_true_if_there_are_is_no_gameFile_file()
        {
            _parsedRomInfo.Quality.Revision.IsRepack = true;

            var remoteRom = Builder<RemoteRom>.CreateNew()
                                                      .With(e => e.ParsedRomInfo = _parsedRomInfo)
                                                      .With(e => e.Roms = _gameFiles)
                                                      .Build();

            Subject.IsSatisfiedBy(remoteRom, null)
                   .Accepted
                   .Should()
                   .BeTrue();
        }

        [Test]
        public void should_return_true_if_is_a_repack_for_a_different_quality()
        {
            _parsedRomInfo.Quality.Revision.IsRepack = true;
            _gameFiles.First().RomFileId = 1;
            _gameFiles.First().RomFile = Builder<RomFile>.CreateNew()
                                                                .With(e => e.Quality = new QualityModel(Quality.DVD))
                                                                .With(e => e.ReleaseGroup = "Romarr")
                                                                .Build();

            var remoteRom = Builder<RemoteRom>.CreateNew()
                                                      .With(e => e.ParsedRomInfo = _parsedRomInfo)
                                                      .With(e => e.Roms = _gameFiles)
                                                      .Build();

            Subject.IsSatisfiedBy(remoteRom, null)
                   .Accepted
                   .Should()
                   .BeTrue();
        }

        [Test]
        public void should_return_true_if_is_a_repack_for_existing_file()
        {
            _parsedRomInfo.Quality.Revision.IsRepack = true;
            _gameFiles.First().RomFileId = 1;
            _gameFiles.First().RomFile = Builder<RomFile>.CreateNew()
                                                                .With(e => e.Quality = new QualityModel(Quality.SDTV))
                                                                .With(e => e.ReleaseGroup = "Romarr")
                                                                .Build();

            var remoteRom = Builder<RemoteRom>.CreateNew()
                                                      .With(e => e.ParsedRomInfo = _parsedRomInfo)
                                                      .With(e => e.Roms = _gameFiles)
                                                      .Build();

            Subject.IsSatisfiedBy(remoteRom, null)
                   .Accepted
                   .Should()
                   .BeTrue();
        }

        [Test]
        public void should_return_false_if_is_a_repack_for_a_different_file()
        {
            _parsedRomInfo.Quality.Revision.IsRepack = true;
            _gameFiles.First().RomFileId = 1;
            _gameFiles.First().RomFile = Builder<RomFile>.CreateNew()
                                                                .With(e => e.Quality = new QualityModel(Quality.SDTV))
                                                                .With(e => e.ReleaseGroup = "NotRomarr")
                                                                .Build();

            var remoteRom = Builder<RemoteRom>.CreateNew()
                                                      .With(e => e.ParsedRomInfo = _parsedRomInfo)
                                                      .With(e => e.Roms = _gameFiles)
                                                      .Build();

            Subject.IsSatisfiedBy(remoteRom, null)
                   .Accepted
                   .Should()
                   .BeFalse();
        }

        [Test]
        public void should_return_false_if_release_group_for_existing_file_is_unknown()
        {
            _parsedRomInfo.Quality.Revision.IsRepack = true;
            _gameFiles.First().RomFileId = 1;
            _gameFiles.First().RomFile = Builder<RomFile>.CreateNew()
                                                                .With(e => e.Quality = new QualityModel(Quality.SDTV))
                                                                .With(e => e.ReleaseGroup = "")
                                                                .Build();

            var remoteRom = Builder<RemoteRom>.CreateNew()
                                                      .With(e => e.ParsedRomInfo = _parsedRomInfo)
                                                      .With(e => e.Roms = _gameFiles)
                                                      .Build();

            Subject.IsSatisfiedBy(remoteRom, null)
                   .Accepted
                   .Should()
                   .BeFalse();
        }

        [Test]
        public void should_return_false_if_release_group_for_release_is_unknown()
        {
            _parsedRomInfo.Quality.Revision.IsRepack = true;
            _parsedRomInfo.ReleaseGroup = null;

            _gameFiles.First().RomFileId = 1;
            _gameFiles.First().RomFile = Builder<RomFile>.CreateNew()
                                                                .With(e => e.Quality = new QualityModel(Quality.SDTV))
                                                                .With(e => e.ReleaseGroup = "Romarr")
                                                                .Build();

            var remoteRom = Builder<RemoteRom>.CreateNew()
                                                      .With(e => e.ParsedRomInfo = _parsedRomInfo)
                                                      .With(e => e.Roms = _gameFiles)
                                                      .Build();

            Subject.IsSatisfiedBy(remoteRom, null)
                   .Accepted
                   .Should()
                   .BeFalse();
        }

        [Test]
        public void should_return_true_when_repacks_are_not_preferred()
        {
            Mocker.GetMock<IConfigService>()
            .Setup(s => s.DownloadPropersAndRepacks)
            .Returns(ProperDownloadTypes.DoNotPrefer);

            _parsedRomInfo.Quality.Revision.IsRepack = true;
            _gameFiles.First().RomFileId = 1;
            _gameFiles.First().RomFile = Builder<RomFile>.CreateNew()
                                                                .With(e => e.Quality = new QualityModel(Quality.SDTV))
                                                                .With(e => e.ReleaseGroup = "Romarr")
                                                                .Build();

            var remoteRom = Builder<RemoteRom>.CreateNew()
                                                      .With(e => e.ParsedRomInfo = _parsedRomInfo)
                                                      .With(e => e.Roms = _gameFiles)
                                                      .Build();

            Subject.IsSatisfiedBy(remoteRom, new()).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_true_when_repack_but_auto_download_repacks_is_true()
        {
            Mocker.GetMock<IConfigService>()
            .Setup(s => s.DownloadPropersAndRepacks)
            .Returns(ProperDownloadTypes.PreferAndUpgrade);

            _parsedRomInfo.Quality.Revision.IsRepack = true;
            _gameFiles.First().RomFileId = 1;
            _gameFiles.First().RomFile = Builder<RomFile>.CreateNew()
                                                                .With(e => e.Quality = new QualityModel(Quality.SDTV))
                                                                .With(e => e.ReleaseGroup = "Romarr")
                                                                .Build();

            var remoteRom = Builder<RemoteRom>.CreateNew()
                                                      .With(e => e.ParsedRomInfo = _parsedRomInfo)
                                                      .With(e => e.Roms = _gameFiles)
                                                      .Build();

            Subject.IsSatisfiedBy(remoteRom, new()).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_false_when_repack_but_auto_download_repacks_is_false()
        {
            Mocker.GetMock<IConfigService>()
            .Setup(s => s.DownloadPropersAndRepacks)
            .Returns(ProperDownloadTypes.DoNotUpgrade);

            _parsedRomInfo.Quality.Revision.IsRepack = true;
            _gameFiles.First().RomFileId = 1;
            _gameFiles.First().RomFile = Builder<RomFile>.CreateNew()
                                                                .With(e => e.Quality = new QualityModel(Quality.SDTV))
                                                                .With(e => e.ReleaseGroup = "Romarr")
                                                                .Build();

            var remoteRom = Builder<RemoteRom>.CreateNew()
                                                      .With(e => e.ParsedRomInfo = _parsedRomInfo)
                                                      .With(e => e.Roms = _gameFiles)
                                                      .Build();

            Subject.IsSatisfiedBy(remoteRom, new()).Accepted.Should().BeFalse();
        }
    }
}
