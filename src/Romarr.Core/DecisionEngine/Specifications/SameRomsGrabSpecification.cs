using NLog;
using Romarr.Core.Parser.Model;

namespace Romarr.Core.DecisionEngine.Specifications
{
    public class SameGameFilesGrabSpecification : IDownloadDecisionEngineSpecification
    {
        private readonly SameFilesSpecification _sameGameFilesSpecification;
        private readonly Logger _logger;

        public SameGameFilesGrabSpecification(SameFilesSpecification sameGameFilesSpecification, Logger logger)
        {
            _sameGameFilesSpecification = sameGameFilesSpecification;
            _logger = logger;
        }

        public SpecificationPriority Priority => SpecificationPriority.Default;
        public RejectionType Type => RejectionType.Permanent;

        public virtual DownloadSpecDecision IsSatisfiedBy(RemoteRom subject, ReleaseDecisionInformation information)
        {
            if (_sameGameFilesSpecification.IsSatisfiedBy(subject.Roms))
            {
                return DownloadSpecDecision.Accept();
            }

            _logger.Debug("Rom file on disk contains more roms than this release contains");
            return DownloadSpecDecision.Reject(DownloadRejectionReason.ExistingFileHasMoreGameFiles, "Rom file on disk contains more roms than this release contains");
        }
    }
}
