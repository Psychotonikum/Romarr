using System.Linq;
using NLog;
using Romarr.Core.Parser.Model;

namespace Romarr.Core.DecisionEngine.Specifications.Search
{
    public class GameFileRequestedSpecification : IDownloadDecisionEngineSpecification
    {
        private readonly Logger _logger;

        public GameFileRequestedSpecification(Logger logger)
        {
            _logger = logger;
        }

        public SpecificationPriority Priority => SpecificationPriority.Default;
        public RejectionType Type => RejectionType.Permanent;

        public DownloadSpecDecision IsSatisfiedBy(RemoteRom remoteRom, ReleaseDecisionInformation information)
        {
            if (information.SearchCriteria == null)
            {
                return DownloadSpecDecision.Accept();
            }

            var criteriaGameFiles = information.SearchCriteria.Roms.Select(v => v.Id).ToList();
            var remoteRoms = remoteRom.Roms.Select(v => v.Id).ToList();

            if (!criteriaGameFiles.Intersect(remoteRoms).Any())
            {
                _logger.Debug("Release rejected since the rom wasn't requested: {0}", remoteRom.ParsedRomInfo);

                if (remoteRoms.Any())
                {
                    var roms = remoteRom.Roms.OrderBy(v => v.PlatformNumber).ThenBy(v => v.FileNumber).ToList();

                    if (roms.Count > 1)
                    {
                        return DownloadSpecDecision.Reject(DownloadRejectionReason.WrongGameFile, $"Rom wasn't requested: {roms.First().PlatformNumber}x{roms.First().FileNumber}-{roms.Last().FileNumber}");
                    }
                    else
                    {
                        return DownloadSpecDecision.Reject(DownloadRejectionReason.WrongGameFile, $"Rom wasn't requested: {roms.First().PlatformNumber}x{roms.First().FileNumber}");
                    }
                }
                else
                {
                    return DownloadSpecDecision.Reject(DownloadRejectionReason.WrongGameFile, "Rom wasn't requested");
                }
            }

            return DownloadSpecDecision.Accept();
        }
    }
}
