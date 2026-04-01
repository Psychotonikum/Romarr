using System.Linq;
using NLog;
using Romarr.Core.DataAugmentation.Scene;
using Romarr.Core.IndexerSearch.Definitions;
using Romarr.Core.Parser.Model;

namespace Romarr.Core.DecisionEngine.Specifications.Search
{
    public class SingleFileSearchMatchSpecification : IDownloadDecisionEngineSpecification
    {
        private readonly Logger _logger;
        private readonly ISceneMappingService _sceneMappingService;

        public SingleFileSearchMatchSpecification(ISceneMappingService sceneMappingService, Logger logger)
        {
            _logger = logger;
            _sceneMappingService = sceneMappingService;
        }

        public SpecificationPriority Priority => SpecificationPriority.Default;
        public RejectionType Type => RejectionType.Permanent;

        public DownloadSpecDecision IsSatisfiedBy(RemoteRom remoteRom, ReleaseDecisionInformation information)
        {
            var searchCriteria = information.SearchCriteria;

            if (searchCriteria == null)
            {
                return DownloadSpecDecision.Accept();
            }

            if (searchCriteria is SingleGameFileSearchCriteria singleGameFileSpec)
            {
                return IsSatisfiedBy(remoteRom, singleGameFileSpec);
            }

            return DownloadSpecDecision.Accept();
        }

        private DownloadSpecDecision IsSatisfiedBy(RemoteRom remoteRom, SingleGameFileSearchCriteria singleGameFileSpec)
        {
            if (singleGameFileSpec.PlatformNumber != remoteRom.ParsedRomInfo.PlatformNumber)
            {
                _logger.Debug("Platform number does not match searched platform number, skipping.");
                return DownloadSpecDecision.Reject(DownloadRejectionReason.WrongPlatform, "Wrong platform");
            }

            if (!remoteRom.ParsedRomInfo.RomNumbers.Any())
            {
                _logger.Debug("Full platform result during single rom search, skipping.");
                return DownloadSpecDecision.Reject(DownloadRejectionReason.FullPlatform, "Full platform pack");
            }

            if (!remoteRom.ParsedRomInfo.RomNumbers.Contains(singleGameFileSpec.FileNumber))
            {
                _logger.Debug("Rom number does not match searched rom number, skipping.");
                return DownloadSpecDecision.Reject(DownloadRejectionReason.WrongGameFile, "Wrong rom");
            }

            return DownloadSpecDecision.Accept();
        }
    }
}
