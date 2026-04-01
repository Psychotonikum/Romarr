using System.Linq;
using NLog;
using Romarr.Core.Download.Pending;
using Romarr.Core.Parser.Model;

namespace Romarr.Core.DecisionEngine.Specifications.RssSync
{
    public class PendingSpecification : IDownloadDecisionEngineSpecification
    {
        private readonly IPendingReleaseService _pendingReleaseService;
        private readonly Logger _logger;

        public PendingSpecification(IPendingReleaseService pendingReleaseService,
                                  Logger logger)
        {
            _pendingReleaseService = pendingReleaseService;
            _logger = logger;
        }

        public SpecificationPriority Priority => SpecificationPriority.Database;
        public RejectionType Type => RejectionType.Temporary;

        public DownloadSpecDecision IsSatisfiedBy(RemoteRom subject, ReleaseDecisionInformation information)
        {
            // Skip this check for RSS sync and interactive searches,

            if (subject.ReleaseSource == ReleaseSourceType.Rss)
            {
                return DownloadSpecDecision.Accept();
            }

            if (information.SearchCriteria is { UserInvokedSearch: true })
            {
                _logger.Debug("Ignoring delay for user invoked search");
                return DownloadSpecDecision.Accept();
            }

            var pending = _pendingReleaseService.GetPendingQueue();

            var matchingGameFile = pending.Where(q => q.RemoteRom?.Game != null &&
                                                     q.RemoteRom.Game.Id == subject.Game.Id &&
                                                     q.RemoteRom.Roms.Select(e => e.Id).Intersect(subject.Roms.Select(e => e.Id)).Any())
                                       .ToList();

            if (matchingGameFile.Any())
            {
                _logger.Debug("Release containing at least one matching rom is already pending. Delaying pushed release");
                return DownloadSpecDecision.Reject(DownloadRejectionReason.MinimumAgeDelayPushed, "Release containing at least one matching rom is already pending, delaying pushed release");
            }

            return DownloadSpecDecision.Accept();
        }
    }
}
