using NLog;
using Romarr.Core.Parser.Model;

namespace Romarr.Core.DecisionEngine.Specifications
{
    public class SplitGameFileSpecification : IDownloadDecisionEngineSpecification
    {
        private readonly Logger _logger;

        public SplitGameFileSpecification(Logger logger)
        {
            _logger = logger;
        }

        public SpecificationPriority Priority => SpecificationPriority.Default;
        public RejectionType Type => RejectionType.Permanent;

        public virtual DownloadSpecDecision IsSatisfiedBy(RemoteRom subject, ReleaseDecisionInformation information)
        {
            if (subject.ParsedRomInfo.IsSplitGameFile)
            {
                _logger.Debug("Split rom release {0} rejected. Not supported", subject.Release.Title);
                return DownloadSpecDecision.Reject(DownloadRejectionReason.SplitGameFile, "Split rom releases are not supported");
            }

            return DownloadSpecDecision.Accept();
        }
    }
}
