using System;
using NLog;
using Romarr.Common.Http;
using Romarr.Core.Configuration;
using Romarr.Core.Localization;
using Romarr.Core.Parser;

namespace Romarr.Core.Indexers.BroadcastheNet
{
    public class BroadcastheNet : HttpIndexerBase<BroadcastheNetSettings>
    {
        public override string Name => "BroadcasTheNet";

        public override DownloadProtocol Protocol => DownloadProtocol.Torrent;
        public override bool SupportsRss => true;
        public override bool SupportsSearch => true;
        public override int PageSize => 100;
        public override TimeSpan RateLimit => TimeSpan.FromSeconds(5);

        public BroadcastheNet(IHttpClient httpClient, IIndexerStatusService indexerStatusService, IConfigService configService, IParsingService parsingService, Logger logger, ILocalizationService localizationService)
            : base(httpClient, indexerStatusService, configService, parsingService, logger, localizationService)
        {
        }

        public override IIndexerRequestGenerator GetRequestGenerator()
        {
            var requestGenerator = new BroadcastheNetRequestGenerator { Settings = Settings, PageSize = PageSize };

            var releaseInfo = _indexerStatusService.GetLastRssSyncReleaseInfo(Definition.Id);

            if (releaseInfo != null && int.TryParse(releaseInfo.Guid.Replace("BTN-", string.Empty), out var torrentId))
            {
                requestGenerator.LastRecentTorrentId = torrentId;
            }

            return requestGenerator;
        }

        public override IParseIndexerResponse GetParser()
        {
            return new BroadcastheNetParser();
        }
    }
}
