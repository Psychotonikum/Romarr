using System.Linq;
using NLog;
using Romarr.Core.Download;
using Romarr.Core.Parser.Model;
namespace Romarr.Core.MediaFiles.GameFileImport.Specifications
{
    public class UnverifiedSceneNumberingSpecification : IImportDecisionEngineSpecification
    {
        private readonly Logger _logger;

        public UnverifiedSceneNumberingSpecification(Logger logger)
        {
            _logger = logger;
        }

        public ImportSpecDecision IsSatisfiedBy(LocalGameFile localRom, DownloadClientItem downloadClientItem)
        {
            if (localRom.ExistingFile)
            {
                _logger.Debug("Skipping scene numbering check for existing rom");
                return ImportSpecDecision.Accept();
            }

            if (localRom.Roms.Any(v => v.UnverifiedSceneNumbering))
            {
                _logger.Debug("This file uses unverified scene numbers, will not auto-import until numbering is confirmed on TheXEM. Skipping {0}", localRom.Path);
                return ImportSpecDecision.Reject(ImportRejectionReason.UnverifiedSceneMapping, "This show has individual rom mappings on TheXEM but the mapping for this rom has not been confirmed yet by their administrators. TheXEM needs manual input.");
            }

            return ImportSpecDecision.Accept();
        }
    }
}
