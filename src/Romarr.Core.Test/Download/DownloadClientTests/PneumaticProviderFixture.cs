using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using FizzWare.NBuilder;
using Moq;
using NLog;
using NUnit.Framework;
using Romarr.Common.Http;
using Romarr.Core.Configuration;
using Romarr.Core.Download;
using Romarr.Core.Download.Clients.Pneumatic;
using Romarr.Core.Indexers;
using Romarr.Core.Localization;
using Romarr.Core.Parser;
using Romarr.Core.Parser.Model;
using Romarr.Core.Test.Framework;
using Romarr.Core.Test.IndexerTests;
using Romarr.Test.Common;

namespace Romarr.Core.Test.Download.DownloadClientTests
{
    [TestFixture]
    public class PneumaticProviderFixture : CoreTest<Pneumatic>
    {
        private const string _nzbUrl = "http://www.nzbs.com/url";
        private const string _title = "30.Rock.S01E05.hdtv.xvid-LoL";
        private string _pneumaticFolder;
        private string _strmFolder;
        private string _nzbPath;
        private RemoteRom _remoteRom;
        private IIndexer _indexer;
        private DownloadClientItem _downloadClientItem;

        [SetUp]
        public void Setup()
        {
            _pneumaticFolder = @"d:\nzb\pneumatic\".AsOsAgnostic();

            _nzbPath = Path.Combine(_pneumaticFolder, _title + ".nzb").AsOsAgnostic();
            _strmFolder = @"d:\unsorted tv\".AsOsAgnostic();

            _remoteRom = new RemoteRom();
            _remoteRom.Release = new ReleaseInfo();
            _remoteRom.Release.Title = _title;
            _remoteRom.Release.DownloadUrl = _nzbUrl;

            _remoteRom.ParsedRomInfo = new ParsedRomInfo();
            _remoteRom.ParsedRomInfo.FullPlatform = false;

            _indexer = new TestIndexer(Mocker.Resolve<IHttpClient>(),
                Mocker.Resolve<IIndexerStatusService>(),
                Mocker.Resolve<IConfigService>(),
                Mocker.Resolve<IParsingService>(),
                Mocker.Resolve<Logger>(),
                Mocker.Resolve<ILocalizationService>());

            _downloadClientItem = Builder<DownloadClientItem>
                                  .CreateNew().With(d => d.DownloadId = "_Droned.S01E01.Pilot.1080p.WEB-DL-DRONE_0")
                                  .Build();

            Subject.Definition = new DownloadClientDefinition();
            Subject.Definition.Settings = new PneumaticSettings
            {
                NzbFolder = _pneumaticFolder,
                StrmFolder = _strmFolder
            };
        }

        private void WithFailedDownload()
        {
            Mocker.GetMock<IHttpClient>().Setup(c => c.DownloadFileAsync(It.IsAny<string>(), It.IsAny<string>())).Throws(new WebException());
        }

        [Test]
        public async Task should_download_file_if_it_doesnt_exist()
        {
            await Subject.Download(_remoteRom, _indexer);

            Mocker.GetMock<IHttpClient>().Verify(c => c.DownloadFileAsync(_nzbUrl, _nzbPath), Times.Once());
        }

        [Test]
        public void should_throw_on_failed_download()
        {
            WithFailedDownload();

            Assert.ThrowsAsync<WebException>(async () => await Subject.Download(_remoteRom, _indexer));
        }

        [Test]
        public void should_throw_if_full_platform_download()
        {
            _remoteRom.Release.Title = "30 Rock - Platform 1";
            _remoteRom.ParsedRomInfo.FullPlatform = true;

            Assert.ThrowsAsync<NotSupportedException>(async () => await Subject.Download(_remoteRom, _indexer));
        }

        [Test]
        public void should_throw_item_is_removed()
        {
            Assert.Throws<NotSupportedException>(() => Subject.RemoveItem(_downloadClientItem, true));
        }

        [Test]
        public async Task should_replace_illegal_characters_in_title()
        {
            var illegalTitle = "Saturday Night Live - S38E08 - Jeremy Renner/Maroon 5 [SDTV]";
            var expectedFilename = Path.Combine(_pneumaticFolder, "Saturday Night Live - S38E08 - Jeremy Renner+Maroon 5 [SDTV].nzb");
            _remoteRom.Release.Title = illegalTitle;

            await Subject.Download(_remoteRom, _indexer);

            Mocker.GetMock<IHttpClient>().Verify(c => c.DownloadFileAsync(It.IsAny<string>(), expectedFilename), Times.Once());
        }
    }
}
