using System;
using System.Linq;
using NLog;
using Romarr.Common.Extensions;
using Romarr.Core.Parser.Model;
using Romarr.Core.Games;

namespace Romarr.Core.DecisionEngine.Specifications
{
    public class PlatformPackOnlySpecification : IDownloadDecisionEngineSpecification
    {
        private readonly Logger _logger;

        public PlatformPackOnlySpecification(Logger logger)
        {
            _logger = logger;
        }

        public SpecificationPriority Priority => SpecificationPriority.Default;
        public RejectionType Type => RejectionType.Permanent;

        public virtual DownloadSpecDecision IsSatisfiedBy(RemoteRom subject, ReleaseDecisionInformation information)
        {
            var searchCriteria = information.SearchCriteria;

            if (searchCriteria == null || searchCriteria.Roms.Count == 1)
            {
                return DownloadSpecDecision.Accept();
            }

            if (subject.Release.PlatformSearchMaximumSingleFileAge > 0)
            {
                if (subject.Game.GameType == GameTypes.Standard && !subject.ParsedRomInfo.FullPlatform && subject.Roms.Count >= 1)
                {
                    // test against roms of the same platform in the current search, and make sure they have an air date
                    var subset = searchCriteria.Roms.Where(e => e.AirDateUtc.HasValue && e.PlatformNumber == subject.Roms.First().PlatformNumber).ToList();

                    if (subset.Count > 0 && subset.Max(e => e.AirDateUtc).Value.Before(DateTime.UtcNow - TimeSpan.FromDays(subject.Release.PlatformSearchMaximumSingleFileAge)))
                    {
                        _logger.Debug("Release {0}: last rom in this platform aired more than {1} days ago, platform pack required.", subject.Release.Title, subject.Release.PlatformSearchMaximumSingleFileAge);
                        return DownloadSpecDecision.Reject(DownloadRejectionReason.NotPlatformPack, "Last rom in this platform aired more than {0} days ago, platform pack required.", subject.Release.PlatformSearchMaximumSingleFileAge);
                    }
                }
            }

            return DownloadSpecDecision.Accept();
        }
    }
}
