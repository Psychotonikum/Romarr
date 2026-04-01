using NLog;
using Romarr.Core.Download;
using Romarr.Core.Parser.Model;

namespace Romarr.Core.MediaFiles.GameFileImport.Specifications
{
    public class SplitGameFileSpecification : IImportDecisionEngineSpecification
    {
        private readonly Logger _logger;

        public SplitGameFileSpecification(Logger logger)
        {
            _logger = logger;
        }

        public ImportSpecDecision IsSatisfiedBy(LocalGameFile localRom, DownloadClientItem downloadClientItem)
        {
            if (localRom.FileRomInfo == null)
            {
                return ImportSpecDecision.Accept();
            }

            if (localRom.FileRomInfo.IsSplitGameFile)
            {
                _logger.Debug("Single rom split into multiple files");
                return ImportSpecDecision.Reject(ImportRejectionReason.SplitGameFile, "Single rom split into multiple files");
            }

            return ImportSpecDecision.Accept();
        }
    }
}
