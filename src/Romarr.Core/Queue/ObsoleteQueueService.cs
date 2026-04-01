using System;
using System.Collections.Generic;
using System.Linq;
using Romarr.Common.Crypto;
using Romarr.Core.Download.TrackedDownloads;
using Romarr.Core.Languages;
using Romarr.Core.Messaging.Events;
using Romarr.Core.Qualities;
using Romarr.Core.Games;

#pragma warning disable CS0612
namespace Romarr.Core.Queue
{
    public interface IObsoleteQueueService
    {
        List<Queue> GetQueue();
        Queue Find(int id);
        void Remove(int id);
    }

    public class ObsoleteQueueService : IObsoleteQueueService, IHandle<TrackedDownloadRefreshedEvent>
    {
        private readonly IEventAggregator _eventAggregator;
        private static List<Queue> _queue = new();

        public ObsoleteQueueService(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
        }

        public List<Queue> GetQueue()
        {
            return _queue;
        }

        public Queue Find(int id)
        {
            return _queue.SingleOrDefault(q => q.Id == id);
        }

        public void Remove(int id)
        {
            _queue.Remove(Find(id));
        }

        private IEnumerable<Queue> MapQueue(TrackedDownload trackedDownload)
        {
            if (trackedDownload.RemoteRom?.Roms != null && trackedDownload.RemoteRom.Roms.Any())
            {
                foreach (var rom in trackedDownload.RemoteRom.Roms)
                {
                    yield return MapQueueItem(trackedDownload, rom);
                }
            }
            else
            {
                yield return MapQueueItem(trackedDownload, null);
            }
        }

        private Queue MapQueueItem(TrackedDownload trackedDownload, Rom rom)
        {
            var queue = new Queue
            {
                Game = trackedDownload.RemoteRom?.Game,
                Rom = rom,
                Languages = trackedDownload.RemoteRom?.Languages ?? new List<Language> { Language.Unknown },
                Quality = trackedDownload.RemoteRom?.ParsedRomInfo.Quality ?? new QualityModel(Quality.Unknown),
                Title = Parser.Parser.RemoveFileExtension(trackedDownload.DownloadItem.Title),
                Size = trackedDownload.DownloadItem.TotalSize,
                SizeLeft = trackedDownload.DownloadItem.RemainingSize,
                TimeLeft = trackedDownload.DownloadItem.RemainingTime,
                Status = Enum.TryParse(trackedDownload.DownloadItem.Status.ToString(), out QueueStatus outValue) ? outValue : QueueStatus.Unknown,
                TrackedDownloadStatus = trackedDownload.Status,
                TrackedDownloadState = trackedDownload.State,
                StatusMessages = trackedDownload.StatusMessages.ToList(),
                ErrorMessage = trackedDownload.DownloadItem.Message,
                RemoteRom = trackedDownload.RemoteRom,
                DownloadId = trackedDownload.DownloadItem.DownloadId,
                Protocol = trackedDownload.Protocol,
                DownloadClient = trackedDownload.DownloadItem.DownloadClientInfo.Name,
                Indexer = trackedDownload.Indexer,
                OutputPath = trackedDownload.DownloadItem.OutputPath.ToString(),
                Added = trackedDownload.Added,
                DownloadClientHasPostImportCategory = trackedDownload.DownloadItem.DownloadClientInfo.HasPostImportCategory
            };

            queue.Id = HashConverter.GetHashInt31($"trackedDownload-{trackedDownload.DownloadClient}-{trackedDownload.DownloadItem.DownloadId}-ep{rom?.Id ?? 0}");

            if (queue.TimeLeft.HasValue)
            {
                queue.EstimatedCompletionTime = DateTime.UtcNow.Add(queue.TimeLeft.Value);
            }

            return queue;
        }

        public void Handle(TrackedDownloadRefreshedEvent message)
        {
            _queue = message.TrackedDownloads
                .Where(t => t.IsTrackable)
                .OrderBy(c => c.DownloadItem.RemainingTime)
                .SelectMany(MapQueue)
                .ToList();

            _eventAggregator.PublishEvent(new ObsoleteQueueUpdatedEvent());
        }
    }
}
#pragma warning restore CS0612
