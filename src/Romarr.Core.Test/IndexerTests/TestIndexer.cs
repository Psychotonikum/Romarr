using NLog;
using Romarr.Common.Http;
using Romarr.Core.Configuration;
using Romarr.Core.Indexers;
using Romarr.Core.Localization;
using Romarr.Core.Parser;

namespace Romarr.Core.Test.IndexerTests
{
    public class TestIndexer : HttpIndexerBase<TestIndexerSettings>
    {
        public override string Name => "Test Indexer";

        public override DownloadProtocol Protocol => DownloadProtocol.Usenet;

        public int _supportedPageSize;
        public override int PageSize => _supportedPageSize;

        public TestIndexer(IHttpClient httpClient, IIndexerStatusService indexerStatusService, IConfigService configService, IParsingService parsingService, Logger logger, ILocalizationService localizationService)
            : base(httpClient, indexerStatusService, configService, parsingService, logger, localizationService)
        {
        }

        public IIndexerRequestGenerator _requestGenerator;
        public override IIndexerRequestGenerator GetRequestGenerator()
        {
            return _requestGenerator;
        }

        public IParseIndexerResponse _parser;
        public override IParseIndexerResponse GetParser()
        {
            return _parser;
        }
    }
}
