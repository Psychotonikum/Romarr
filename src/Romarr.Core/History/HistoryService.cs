using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NLog;
using Romarr.Common.Extensions;
using Romarr.Core.Datastore;
using Romarr.Core.Download;
using Romarr.Core.MediaFiles;
using Romarr.Core.MediaFiles.Events;
using Romarr.Core.Messaging.Events;
using Romarr.Core.Parser.Model;
using Romarr.Core.Games.Events;

namespace Romarr.Core.History
{
    public interface IHistoryService
    {
        PagingSpec<FileHistory> Paged(PagingSpec<FileHistory> pagingSpec, int[] languages, int[] qualities);
        FileHistory MostRecentForRom(int romId);
        List<FileHistory> FindByRomId(int romId);
        FileHistory MostRecentForDownloadId(string downloadId);
        FileHistory Get(int historyId);
        List<FileHistory> GetBySeries(int gameId, FileHistoryEventType? eventType);
        List<FileHistory> GetByPlatform(int gameId, int platformNumber, FileHistoryEventType? eventType);
        List<FileHistory> GetByGameFile(int romId, FileHistoryEventType? eventType);
        List<FileHistory> Find(string downloadId, FileHistoryEventType eventType);
        List<FileHistory> FindByDownloadId(string downloadId);
        string FindDownloadId(FileImportedEvent trackedDownload);
        List<FileHistory> Since(DateTime date, FileHistoryEventType? eventType);
    }

    public class HistoryService : IHistoryService,
                                  IHandle<FileGrabbedEvent>,
                                  IHandle<FileImportedEvent>,
                                  IHandle<DownloadFailedEvent>,
                                  IHandle<RomFileDeletedEvent>,
                                  IHandle<RomFileRenamedEvent>,
                                  IHandle<GameDeletedEvent>,
                                  IHandle<DownloadIgnoredEvent>
    {
        private readonly IHistoryRepository _historyRepository;
        private readonly Logger _logger;

        public HistoryService(IHistoryRepository historyRepository, Logger logger)
        {
            _historyRepository = historyRepository;
            _logger = logger;
        }

        public PagingSpec<FileHistory> Paged(PagingSpec<FileHistory> pagingSpec, int[] languages, int[] qualities)
        {
            return _historyRepository.GetPaged(pagingSpec, languages, qualities);
        }

        public FileHistory MostRecentForRom(int romId)
        {
            return _historyRepository.MostRecentForRom(romId);
        }

        public List<FileHistory> FindByRomId(int romId)
        {
            return _historyRepository.FindByRomId(romId);
        }

        public FileHistory MostRecentForDownloadId(string downloadId)
        {
            return _historyRepository.MostRecentForDownloadId(downloadId);
        }

        public FileHistory Get(int historyId)
        {
            return _historyRepository.Get(historyId);
        }

        public List<FileHistory> GetBySeries(int gameId, FileHistoryEventType? eventType)
        {
            return _historyRepository.GetBySeries(gameId, eventType);
        }

        public List<FileHistory> GetByPlatform(int gameId, int platformNumber, FileHistoryEventType? eventType)
        {
            return _historyRepository.GetByPlatform(gameId, platformNumber, eventType);
        }

        public List<FileHistory> GetByGameFile(int romId, FileHistoryEventType? eventType)
        {
            return _historyRepository.GetByGameFile(romId, eventType);
        }

        public List<FileHistory> Find(string downloadId, FileHistoryEventType eventType)
        {
            return _historyRepository.FindByDownloadId(downloadId).Where(c => c.EventType == eventType).ToList();
        }

        public List<FileHistory> FindByDownloadId(string downloadId)
        {
            return _historyRepository.FindByDownloadId(downloadId);
        }

        public string FindDownloadId(FileImportedEvent trackedDownload)
        {
            _logger.Debug("Trying to find downloadId for {0} from history", trackedDownload.ImportedGameFile.Path);

            var romIds = trackedDownload.RomInfo.Roms.Select(c => c.Id).ToList();
            var allHistory = _historyRepository.FindDownloadHistory(trackedDownload.RomInfo.Game.Id, trackedDownload.ImportedGameFile.Quality);

            // Find download related items for these roms
            var gameFilesHistory = allHistory.Where(h => romIds.Contains(h.FileId)).ToList();

            var processedDownloadId = gameFilesHistory
                .Where(c => c.EventType != FileHistoryEventType.Grabbed && c.DownloadId != null)
                .Select(c => c.DownloadId);

            var stillDownloading = gameFilesHistory.Where(c => c.EventType == FileHistoryEventType.Grabbed && !processedDownloadId.Contains(c.DownloadId)).ToList();

            string downloadId = null;

            if (stillDownloading.Any())
            {
                foreach (var matchingHistory in trackedDownload.RomInfo.Roms.Select(e => stillDownloading.Where(c => c.FileId == e.Id).ToList()))
                {
                    if (matchingHistory.Count != 1)
                    {
                        return null;
                    }

                    var newDownloadId = matchingHistory.Single().DownloadId;

                    if (downloadId == null || downloadId == newDownloadId)
                    {
                        downloadId = newDownloadId;
                    }
                    else
                    {
                        return null;
                    }
                }
            }

            return downloadId;
        }

        public void Handle(FileGrabbedEvent message)
        {
            foreach (var rom in message.Rom.Roms)
            {
                var history = new FileHistory
                {
                    EventType = FileHistoryEventType.Grabbed,
                    Date = DateTime.UtcNow,
                    Quality = message.Rom.ParsedRomInfo.Quality,
                    SourceTitle = message.Rom.Release.Title,
                    GameId = rom.GameId,
                    FileId = rom.Id,
                    DownloadId = message.DownloadId,
                    Languages = message.Rom.Languages,
                };

                history.Data.Add("Indexer", message.Rom.Release.Indexer);
                history.Data.Add("NzbInfoUrl", message.Rom.Release.InfoUrl);
                history.Data.Add("ReleaseGroup", message.Rom.ParsedRomInfo.ReleaseGroup);
                history.Data.Add("Age", message.Rom.Release.Age.ToString());
                history.Data.Add("AgeHours", message.Rom.Release.AgeHours.ToString());
                history.Data.Add("AgeMinutes", message.Rom.Release.AgeMinutes.ToString());
                history.Data.Add("PublishedDate", message.Rom.Release.PublishDate.ToUniversalTime().ToString("s") + "Z");
                history.Data.Add("DownloadClient", message.DownloadClient);
                history.Data.Add("DownloadClientName", message.DownloadClientName);
                history.Data.Add("Size", message.Rom.Release.Size.ToString());
                history.Data.Add("DownloadUrl", message.Rom.Release.DownloadUrl);
                history.Data.Add("Guid", message.Rom.Release.Guid);
                history.Data.Add("IgdbId", message.Rom.Release.IgdbId.ToString());
                history.Data.Add("MobyGamesId", message.Rom.Release.MobyGamesId.ToString());
                history.Data.Add("ImdbId", message.Rom.Release.ImdbId);
                history.Data.Add("Protocol", ((int)message.Rom.Release.DownloadProtocol).ToString());
                history.Data.Add("CustomFormatScore", message.Rom.CustomFormatScore.ToString());
                history.Data.Add("SeriesMatchType", message.Rom.SeriesMatchType.ToString());
                history.Data.Add("ReleaseSource", message.Rom.ReleaseSource.ToString());
                history.Data.Add("IndexerFlags", message.Rom.Release.IndexerFlags.ToString());
                history.Data.Add("ReleaseType", message.Rom.ParsedRomInfo.ReleaseType.ToString());

                if (!message.Rom.ParsedRomInfo.ReleaseHash.IsNullOrWhiteSpace())
                {
                    history.Data.Add("ReleaseHash", message.Rom.ParsedRomInfo.ReleaseHash);
                }

                if (message.Rom.Release is TorrentInfo torrentRelease)
                {
                    history.Data.Add("TorrentInfoHash", torrentRelease.InfoHash);
                }

                _historyRepository.Insert(history);
            }
        }

        public void Handle(FileImportedEvent message)
        {
            if (!message.NewDownload)
            {
                return;
            }

            var downloadId = message.DownloadId;

            if (downloadId.IsNullOrWhiteSpace())
            {
                downloadId = FindDownloadId(message);
            }

            foreach (var rom in message.RomInfo.Roms)
            {
                var history = new FileHistory
                {
                    EventType = FileHistoryEventType.DownloadFolderImported,
                    Date = DateTime.UtcNow,
                    Quality = message.RomInfo.Quality,
                    SourceTitle = message.ImportedGameFile.SceneName ?? Path.GetFileNameWithoutExtension(message.RomInfo.Path),
                    GameId = message.ImportedGameFile.GameId,
                    FileId = rom.Id,
                    DownloadId = downloadId,
                    Languages = message.RomInfo.Languages
                };

                history.Data.Add("FileId", message.ImportedGameFile.Id.ToString());
                history.Data.Add("DroppedPath", message.RomInfo.Path);
                history.Data.Add("ImportedPath", Path.Combine(message.RomInfo.Game.Path, message.ImportedGameFile.RelativePath));
                history.Data.Add("DownloadClient", message.DownloadClientInfo?.Type);
                history.Data.Add("DownloadClientName", message.DownloadClientInfo?.Name);
                history.Data.Add("ReleaseGroup", message.RomInfo.ReleaseGroup);
                history.Data.Add("CustomFormatScore", message.RomInfo.CustomFormatScore.ToString());
                history.Data.Add("Size", message.RomInfo.Size.ToString());
                history.Data.Add("IndexerFlags", message.ImportedGameFile.IndexerFlags.ToString());
                history.Data.Add("ReleaseType", message.ImportedGameFile.ReleaseType.ToString());

                _historyRepository.Insert(history);
            }
        }

        public void Handle(DownloadFailedEvent message)
        {
            foreach (var romId in message.RomIds)
            {
                var history = new FileHistory
                {
                    EventType = FileHistoryEventType.DownloadFailed,
                    Date = DateTime.UtcNow,
                    Quality = message.Quality,
                    SourceTitle = message.SourceTitle,
                    GameId = message.GameId,
                    FileId = romId,
                    DownloadId = message.DownloadId,
                    Languages = message.Languages
                };

                history.Data.Add("DownloadClient", message.DownloadClient);
                history.Data.Add("DownloadClientName", message.TrackedDownload?.DownloadItem.DownloadClientInfo.Name);
                history.Data.Add("Message", message.Message);
                history.Data.Add("Source", message.Source);
                history.Data.Add("ReleaseGroup", message.TrackedDownload?.RemoteRom?.ParsedRomInfo?.ReleaseGroup ?? message.Data.GetValueOrDefault(FileHistory.RELEASE_GROUP));
                history.Data.Add("Size", message.TrackedDownload?.DownloadItem.TotalSize.ToString() ?? message.Data.GetValueOrDefault(FileHistory.SIZE));
                history.Data.Add("Indexer", message.TrackedDownload?.RemoteRom?.Release?.Indexer ?? message.Data.GetValueOrDefault(FileHistory.INDEXER));

                _historyRepository.Insert(history);
            }
        }

        public void Handle(RomFileDeletedEvent message)
        {
            if (message.Reason == DeleteMediaFileReason.NoLinkedGameFiles)
            {
                _logger.Debug("Removing rom file from DB as part of cleanup routine, not creating history event.");
                return;
            }
            else if (message.Reason == DeleteMediaFileReason.ManualOverride)
            {
                _logger.Debug("Removing rom file from DB as part of manual override of existing file, not creating history event.");
                return;
            }

            foreach (var rom in message.RomFile.Roms.Value)
            {
                var history = new FileHistory
                {
                    EventType = FileHistoryEventType.RomFileDeleted,
                    Date = DateTime.UtcNow,
                    Quality = message.RomFile.Quality,
                    SourceTitle = message.RomFile.Path,
                    GameId = message.RomFile.GameId,
                    FileId = rom.Id,
                    Languages = message.RomFile.Languages
                };

                history.Data.Add("Reason", message.Reason.ToString());
                history.Data.Add("ReleaseGroup", message.RomFile.ReleaseGroup);
                history.Data.Add("Size", message.RomFile.Size.ToString());
                history.Data.Add("IndexerFlags", message.RomFile.IndexerFlags.ToString());
                history.Data.Add("ReleaseType", message.RomFile.ReleaseType.ToString());

                _historyRepository.Insert(history);
            }
        }

        public void Handle(RomFileRenamedEvent message)
        {
            var sourcePath = message.OriginalPath;
            var sourceRelativePath = message.Game.Path.GetRelativePath(message.OriginalPath);
            var path = Path.Combine(message.Game.Path, message.RomFile.RelativePath);
            var relativePath = message.RomFile.RelativePath;

            foreach (var rom in message.RomFile.Roms.Value)
            {
                var history = new FileHistory
                {
                    EventType = FileHistoryEventType.RomFileRenamed,
                    Date = DateTime.UtcNow,
                    Quality = message.RomFile.Quality,
                    SourceTitle = message.OriginalPath,
                    GameId = message.RomFile.GameId,
                    FileId = rom.Id,
                    Languages = message.RomFile.Languages
                };

                history.Data.Add("SourcePath", sourcePath);
                history.Data.Add("SourceRelativePath", sourceRelativePath);
                history.Data.Add("Path", path);
                history.Data.Add("RelativePath", relativePath);
                history.Data.Add("ReleaseGroup", message.RomFile.ReleaseGroup);
                history.Data.Add("Size", message.RomFile.Size.ToString());
                history.Data.Add("IndexerFlags", message.RomFile.IndexerFlags.ToString());
                history.Data.Add("ReleaseType", message.RomFile.ReleaseType.ToString());

                _historyRepository.Insert(history);
            }
        }

        public void Handle(DownloadIgnoredEvent message)
        {
            var historyToAdd = new List<FileHistory>();

            foreach (var romId in message.RomIds)
            {
                var history = new FileHistory
                {
                    EventType = FileHistoryEventType.DownloadIgnored,
                    Date = DateTime.UtcNow,
                    Quality = message.Quality,
                    SourceTitle = message.SourceTitle,
                    GameId = message.GameId,
                    FileId = romId,
                    DownloadId = message.DownloadId,
                    Languages = message.Languages
                };

                history.Data.Add("DownloadClient", message.DownloadClientInfo.Type);
                history.Data.Add("DownloadClientName", message.DownloadClientInfo.Name);
                history.Data.Add("Message", message.Message);
                history.Data.Add("ReleaseGroup", message.TrackedDownload?.RemoteRom?.ParsedRomInfo?.ReleaseGroup);
                history.Data.Add("Size", message.TrackedDownload?.DownloadItem.TotalSize.ToString());
                history.Data.Add("Indexer", message.TrackedDownload?.RemoteRom?.Release?.Indexer);
                history.Data.Add("ReleaseType", message.TrackedDownload?.RemoteRom?.ParsedRomInfo?.ReleaseType.ToString());

                historyToAdd.Add(history);
            }

            _historyRepository.InsertMany(historyToAdd);
        }

        public void Handle(GameDeletedEvent message)
        {
            _historyRepository.DeleteForSeries(message.Game.Select(m => m.Id).ToList());
        }

        public List<FileHistory> Since(DateTime date, FileHistoryEventType? eventType)
        {
            return _historyRepository.Since(date, eventType);
        }
    }
}
