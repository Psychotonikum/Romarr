using NLog;
using Romarr.Common.Instrumentation.Extensions;
using Romarr.Core.Download;
using Romarr.Core.Messaging.Commands;

namespace Romarr.Core.IndexerSearch
{
    public class PlatformSearchService : IExecute<PlatformSearchCommand>
    {
        private readonly ISearchForReleases _releaseSearchService;
        private readonly IProcessDownloadDecisions _processDownloadDecisions;
        private readonly Logger _logger;

        public PlatformSearchService(ISearchForReleases releaseSearchService,
                                   IProcessDownloadDecisions processDownloadDecisions,
                                   Logger logger)
        {
            _releaseSearchService = releaseSearchService;
            _processDownloadDecisions = processDownloadDecisions;
            _logger = logger;
        }

        public void Execute(PlatformSearchCommand message)
        {
            var decisions = _releaseSearchService.PlatformSearch(message.GameId, message.PlatformNumber, false, true, message.Trigger == CommandTrigger.Manual, false).GetAwaiter().GetResult();
            var processed = _processDownloadDecisions.ProcessDecisions(decisions).GetAwaiter().GetResult();

            _logger.ProgressInfo("Platform search completed. {0} reports downloaded.", processed.Grabbed.Count);
        }
    }
}
