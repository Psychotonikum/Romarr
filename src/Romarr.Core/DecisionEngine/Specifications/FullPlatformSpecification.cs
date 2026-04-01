using System;
using System.Linq;
using NLog;
using Romarr.Common.Extensions;
using Romarr.Core.Parser.Model;

namespace Romarr.Core.DecisionEngine.Specifications
{
    public class FullPlatformSpecification : IDownloadDecisionEngineSpecification
    {
        private readonly Logger _logger;

        public FullPlatformSpecification(Logger logger)
        {
            _logger = logger;
        }

        public SpecificationPriority Priority => SpecificationPriority.Default;
        public RejectionType Type => RejectionType.Permanent;

        public virtual DownloadSpecDecision IsSatisfiedBy(RemoteRom subject, ReleaseDecisionInformation information)
        {
            if (subject.ParsedRomInfo.FullPlatform)
            {
                _logger.Debug("Checking if all roms in full platform release have aired. {0}", subject.Release.Title);

                if (subject.Roms.Any(e => !e.AirDateUtc.HasValue || e.AirDateUtc.Value.After(DateTime.UtcNow.AddHours(24))))
                {
                    _logger.Debug("Full platform release {0} rejected. All roms haven't aired yet.", subject.Release.Title);
                    return DownloadSpecDecision.Reject(DownloadRejectionReason.FullPlatformNotAired, "Full platform release rejected. All roms haven't aired yet.");
                }
            }

            return DownloadSpecDecision.Accept();
        }
    }
}
