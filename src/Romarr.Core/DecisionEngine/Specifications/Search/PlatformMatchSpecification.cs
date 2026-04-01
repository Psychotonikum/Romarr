using NLog;
using Romarr.Core.DataAugmentation.Scene;
using Romarr.Core.IndexerSearch.Definitions;
using Romarr.Core.Parser.Model;

namespace Romarr.Core.DecisionEngine.Specifications.Search
{
    public class PlatformMatchSpecification : IDownloadDecisionEngineSpecification
    {
        private readonly Logger _logger;
        private readonly ISceneMappingService _sceneMappingService;

        public PlatformMatchSpecification(ISceneMappingService sceneMappingService, Logger logger)
        {
            _logger = logger;
            _sceneMappingService = sceneMappingService;
        }

        public SpecificationPriority Priority => SpecificationPriority.Default;
        public RejectionType Type => RejectionType.Permanent;

        public DownloadSpecDecision IsSatisfiedBy(RemoteRom remoteRom, ReleaseDecisionInformation information)
        {
            if (information.SearchCriteria == null)
            {
                return DownloadSpecDecision.Accept();
            }

            var singleGameFileSpec = information.SearchCriteria as PlatformSearchCriteria;

            if (singleGameFileSpec == null)
            {
                return DownloadSpecDecision.Accept();
            }

            if (singleGameFileSpec.PlatformNumber != remoteRom.ParsedRomInfo.PlatformNumber)
            {
                _logger.Debug("Platform number does not match searched platform number, skipping.");
                return DownloadSpecDecision.Reject(DownloadRejectionReason.WrongPlatform, "Wrong platform");
            }

            return DownloadSpecDecision.Accept();
        }
    }
}
