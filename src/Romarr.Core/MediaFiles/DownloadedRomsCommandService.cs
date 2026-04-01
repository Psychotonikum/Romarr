using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using Romarr.Common.Disk;
using Romarr.Common.Extensions;
using Romarr.Common.Instrumentation.Extensions;
using Romarr.Core.Download;
using Romarr.Core.Download.TrackedDownloads;
using Romarr.Core.MediaFiles.Commands;
using Romarr.Core.MediaFiles.GameFileImport;
using Romarr.Core.Messaging.Commands;

namespace Romarr.Core.MediaFiles
{
    public class DownloadedGameFilesCommandService : IExecute<DownloadedGameFilesScanCommand>
    {
        private readonly IDownloadedFilesImportService _downloadedGameFilesImportService;
        private readonly ITrackedDownloadService _trackedDownloadService;
        private readonly IDiskProvider _diskProvider;
        private readonly ICompletedDownloadService _completedDownloadService;
        private readonly ICommandResultReporter _commandResultReporter;
        private readonly Logger _logger;

        public DownloadedGameFilesCommandService(IDownloadedFilesImportService downloadedGameFilesImportService,
                                                ITrackedDownloadService trackedDownloadService,
                                                IDiskProvider diskProvider,
                                                ICompletedDownloadService completedDownloadService,
                                                ICommandResultReporter commandResultReporter,
                                                Logger logger)
        {
            _downloadedGameFilesImportService = downloadedGameFilesImportService;
            _trackedDownloadService = trackedDownloadService;
            _diskProvider = diskProvider;
            _completedDownloadService = completedDownloadService;
            _commandResultReporter = commandResultReporter;
            _logger = logger;
        }

        private List<ImportResult> ProcessPath(DownloadedGameFilesScanCommand message)
        {
            if (!_diskProvider.FolderExists(message.Path) && !_diskProvider.FileExists(message.Path))
            {
                _logger.Warn("Folder/File specified for import scan [{0}] doesn't exist.", message.Path);
                return new List<ImportResult>();
            }

            if (message.DownloadClientId.IsNotNullOrWhiteSpace())
            {
                var trackedDownload = _trackedDownloadService.Find(message.DownloadClientId);

                if (trackedDownload != null)
                {
                    _logger.Debug("External directory scan request for known download {0}. [{1}]", message.DownloadClientId, message.Path);

                    var importResults = _downloadedGameFilesImportService.ProcessPath(message.Path, message.ImportMode, trackedDownload.RemoteRom.Game, trackedDownload.DownloadItem);

                    _completedDownloadService.VerifyImport(trackedDownload, importResults);

                    return importResults;
                }

                _logger.Warn("External directory scan request for unknown download {0}, attempting normal import. [{1}]", message.DownloadClientId, message.Path);
            }

            return _downloadedGameFilesImportService.ProcessPath(message.Path, message.ImportMode);
        }

        public void Execute(DownloadedGameFilesScanCommand message)
        {
            List<ImportResult> importResults;

            if (message.Path.IsNotNullOrWhiteSpace())
            {
                importResults = ProcessPath(message);
            }
            else
            {
                throw new ArgumentException("A path must be provided", "path");
            }

            if (importResults == null || importResults.All(v => v.Result != ImportResultType.Imported))
            {
                // Allow the command to complete successfully, but report as unsuccessful

                _logger.ProgressDebug("Failed to import");
                _commandResultReporter.Report(CommandResult.Unsuccessful);
            }
        }
    }
}
