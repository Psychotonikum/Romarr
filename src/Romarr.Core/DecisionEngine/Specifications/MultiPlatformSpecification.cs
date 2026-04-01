using NLog;
using Romarr.Core.Parser.Model;

namespace Romarr.Core.DecisionEngine.Specifications
{
    public class MultiPlatformSpecification : IDownloadDecisionEngineSpecification
    {
        private readonly Logger _logger;

        public MultiPlatformSpecification(Logger logger)
        {
            _logger = logger;
        }

        public SpecificationPriority Priority => SpecificationPriority.Default;
        public RejectionType Type => RejectionType.Permanent;

        public virtual DownloadSpecDecision IsSatisfiedBy(RemoteRom subject, ReleaseDecisionInformation information)
        {
            if (subject.ParsedRomInfo.IsMultiPlatform)
            {
                _logger.Debug("Multi-platform release {0} rejected. Not supported", subject.Release.Title);
                return DownloadSpecDecision.Reject(DownloadRejectionReason.MultiPlatform, "Multi-platform releases are not supported");
            }

            return DownloadSpecDecision.Accept();
        }
    }
}
