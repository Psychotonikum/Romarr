using NLog;
using Romarr.Core.DecisionEngine;
using Romarr.Core.DecisionEngine.Specifications;
using Romarr.Core.Download;
using Romarr.Core.Parser.Model;

namespace Romarr.Core.MediaFiles.GameFileImport.Specifications
{
    public class SameGameFilesImportSpecification : IImportDecisionEngineSpecification
    {
        private readonly SameFilesSpecification _sameGameFilesSpecification;
        private readonly Logger _logger;

        public SameGameFilesImportSpecification(SameFilesSpecification sameGameFilesSpecification, Logger logger)
        {
            _sameGameFilesSpecification = sameGameFilesSpecification;
            _logger = logger;
        }

        public RejectionType Type => RejectionType.Permanent;

        public ImportSpecDecision IsSatisfiedBy(LocalGameFile localRom, DownloadClientItem downloadClientItem)
        {
            if (_sameGameFilesSpecification.IsSatisfiedBy(localRom.Roms))
            {
                return ImportSpecDecision.Accept();
            }

            _logger.Debug("Rom file on disk contains more roms than this file contains");
            return ImportSpecDecision.Reject(ImportRejectionReason.ExistingFileHasMoreGameFiles, "Rom file on disk contains more roms than this file contains");
        }
    }
}
