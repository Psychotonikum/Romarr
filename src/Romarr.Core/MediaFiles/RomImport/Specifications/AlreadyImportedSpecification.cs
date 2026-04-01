using System.Linq;
using NLog;
using Romarr.Common.Extensions;
using Romarr.Core.DecisionEngine;
using Romarr.Core.Download;
using Romarr.Core.History;
using Romarr.Core.Parser.Model;

namespace Romarr.Core.MediaFiles.GameFileImport.Specifications
{
    public class AlreadyImportedSpecification : IImportDecisionEngineSpecification
    {
        private readonly IHistoryService _historyService;
        private readonly Logger _logger;

        public AlreadyImportedSpecification(IHistoryService historyService,
                                            Logger logger)
        {
            _historyService = historyService;
            _logger = logger;
        }

        public SpecificationPriority Priority => SpecificationPriority.Database;

        public ImportSpecDecision IsSatisfiedBy(LocalGameFile localRom, DownloadClientItem downloadClientItem)
        {
            if (downloadClientItem == null)
            {
                _logger.Debug("No download client information is available, skipping");
                return ImportSpecDecision.Accept();
            }

            foreach (var rom in localRom.Roms)
            {
                if (!rom.HasFile)
                {
                    _logger.Debug("Skipping already imported check for rom without file");
                    continue;
                }

                var gameFileHistory = _historyService.FindByRomId(rom.Id);
                var lastImported = gameFileHistory.FirstOrDefault(h =>
                    h.DownloadId == downloadClientItem.DownloadId &&
                    h.EventType == FileHistoryEventType.DownloadFolderImported);
                var lastGrabbed = gameFileHistory.FirstOrDefault(h =>
                    h.DownloadId == downloadClientItem.DownloadId && h.EventType == FileHistoryEventType.Grabbed);

                if (lastImported == null)
                {
                    _logger.Trace("Rom file has not been imported");
                    continue;
                }

                if (lastGrabbed != null)
                {
                    // If the release was grabbed again after importing don't reject it
                    if (lastGrabbed.Date.After(lastImported.Date))
                    {
                        _logger.Trace("Rom file was grabbed again after importing");
                        continue;
                    }

                    // If the release was imported after the last grab reject it
                    if (lastImported.Date.After(lastGrabbed.Date))
                    {
                        _logger.Debug("Rom file previously imported at {0}", lastImported.Date);
                        return ImportSpecDecision.Reject(ImportRejectionReason.GameFileAlreadyImported, "Rom file already imported at {0}", lastImported.Date.ToLocalTime());
                    }
                }
                else
                {
                    _logger.Debug("Rom file previously imported at {0}", lastImported.Date);
                    return ImportSpecDecision.Reject(ImportRejectionReason.GameFileAlreadyImported, "Rom file already imported at {0}", lastImported.Date.ToLocalTime());
                }
            }

            return ImportSpecDecision.Accept();
        }
    }
}
