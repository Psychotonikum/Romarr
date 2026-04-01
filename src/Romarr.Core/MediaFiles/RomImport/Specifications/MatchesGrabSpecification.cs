using System.Collections.Generic;
using System.Linq;
using NLog;
using Romarr.Common.Extensions;
using Romarr.Core.Download;
using Romarr.Core.Parser.Model;
using Romarr.Core.Games;

namespace Romarr.Core.MediaFiles.GameFileImport.Specifications
{
    public class MatchesGrabSpecification : IImportDecisionEngineSpecification
    {
        private readonly Logger _logger;

        public MatchesGrabSpecification(Logger logger)
        {
            _logger = logger;
        }

        public ImportSpecDecision IsSatisfiedBy(LocalGameFile localRom, DownloadClientItem downloadClientItem)
        {
            if (localRom.ExistingFile)
            {
                return ImportSpecDecision.Accept();
            }

            var releaseInfo = localRom.Release;

            if (releaseInfo == null || releaseInfo.RomIds.Empty())
            {
                return ImportSpecDecision.Accept();
            }

            var unexpected = localRom.Roms.Where(e => releaseInfo.RomIds.All(o => o != e.Id)).ToList();

            if (unexpected.Any())
            {
                _logger.Debug("Unexpected rom(s) in file: {0}", FormatGameFile(unexpected));

                if (unexpected.Count == 1)
                {
                    return ImportSpecDecision.Reject(ImportRejectionReason.GameFileNotFoundInRelease, "Rom {0} was not found in the grabbed release: {1}", FormatGameFile(unexpected), releaseInfo.Title);
                }

                return ImportSpecDecision.Reject(ImportRejectionReason.GameFileNotFoundInRelease, "Roms {0} were not found in the grabbed release: {1}", FormatGameFile(unexpected), releaseInfo.Title);
            }

            return ImportSpecDecision.Accept();
        }

        private string FormatGameFile(List<Rom> roms)
        {
            return string.Join(", ", roms.Select(e => $"{e.PlatformNumber}x{e.FileNumber:00}"));
        }
    }
}
