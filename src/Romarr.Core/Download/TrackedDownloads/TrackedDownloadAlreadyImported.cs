using System.Collections.Generic;
using System.Linq;
using NLog;
using Romarr.Common.Extensions;
using Romarr.Core.History;

namespace Romarr.Core.Download.TrackedDownloads
{
    public interface ITrackedDownloadAlreadyImported
    {
        bool IsImported(TrackedDownload trackedDownload, List<FileHistory> historyItems);
    }

    public class TrackedDownloadAlreadyImported : ITrackedDownloadAlreadyImported
    {
        private readonly Logger _logger;

        public TrackedDownloadAlreadyImported(Logger logger)
        {
            _logger = logger;
        }

        public bool IsImported(TrackedDownload trackedDownload, List<FileHistory> historyItems)
        {
            _logger.Trace("Checking if all roms for '{0}' have been imported", trackedDownload.DownloadItem.Title);

            if (historyItems.Empty())
            {
                _logger.Trace("No history for {0}", trackedDownload.DownloadItem.Title);
                return false;
            }

            var allGameFilesImportedInHistory = trackedDownload.RemoteRom.Roms.All(e =>
            {
                var lastHistoryItem = historyItems.FirstOrDefault(h => h.FileId == e.Id);

                if (lastHistoryItem == null)
                {
                    _logger.Trace("No history for rom: S{0:00}E{1:00} [{2}]", e.PlatformNumber, e.FileNumber, e.Id);
                    return false;
                }

                _logger.Trace("Last event for rom: S{0:00}E{1:00} [{2}] is: {3}", e.PlatformNumber, e.FileNumber, e.Id, lastHistoryItem.EventType);

                return lastHistoryItem.EventType == FileHistoryEventType.DownloadFolderImported;
            });

            _logger.Trace("All roms for '{0}' have been imported: {1}", trackedDownload.DownloadItem.Title, allGameFilesImportedInHistory);

            return allGameFilesImportedInHistory;
        }
    }
}
