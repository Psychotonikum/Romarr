using Romarr.Core.Download.TrackedDownloads;
using Romarr.Core.Indexers;
using Romarr.Core.Languages;
using Romarr.Core.Qualities;
using Romarr.Core.Queue;
using Romarr.Api.V5.CustomFormats;
using Romarr.Api.V5.Roms;
using Romarr.Api.V5.Game;
using Romarr.Http.REST;

namespace Romarr.Api.V5.Queue
{
    public class QueueResource : RestResource
    {
        public int? GameId { get; set; }
        public IEnumerable<int> RomIds { get; set; } = [];
        public List<int> PlatformNumbers { get; set; } = [];
        public GameResource? Game { get; set; }
        public List<RomResource>? Roms { get; set; }
        public List<Language> Languages { get; set; } = [];
        public QualityModel Quality { get; set; } = new(Romarr.Core.Qualities.Quality.Unknown);
        public List<CustomFormatResource> CustomFormats { get; set; } = [];
        public int CustomFormatScore { get; set; }
        public decimal Size { get; set; }
        public string? Title { get; set; }
        public decimal SizeLeft { get; set; }
        public TimeSpan? TimeLeft { get; set; }
        public DateTime? EstimatedCompletionTime { get; set; }
        public DateTime? Added { get; set; }
        public QueueStatus Status { get; set; }
        public TrackedDownloadStatus? TrackedDownloadStatus { get; set; }
        public TrackedDownloadState? TrackedDownloadState { get; set; }
        public List<TrackedDownloadStatusMessage>? StatusMessages { get; set; }
        public string? ErrorMessage { get; set; }
        public string? DownloadId { get; set; }
        public DownloadProtocol Protocol { get; set; }
        public string? DownloadClient { get; set; }
        public bool DownloadClientHasPostImportCategory { get; set; }
        public string? Indexer { get; set; }
        public string? OutputPath { get; set; }
        public int GameFilesWithFilesCount { get; set; }
        public bool IsFullPlatform { get; set; }
    }

    public static class QueueResourceMapper
    {
        public static QueueResource ToResource(this Romarr.Core.Queue.Queue model, bool includeSeries, bool includeGameFiles)
        {
            var customFormats = model.RemoteRom?.CustomFormats;
            var customFormatScore = model.Game?.QualityProfile?.Value?.CalculateCustomFormatScore(customFormats) ?? 0;

            return new QueueResource
            {
                Id = model.Id,
                GameId = model.Game?.Id,
                RomIds = model.Roms?.Select(e => e.Id).ToList() ?? [],
                PlatformNumbers = model.PlatformNumber.HasValue ? [model.PlatformNumber.Value] : [],
                Game = includeSeries && model.Game != null ? model.Game.ToResource() : null,
                Roms = includeGameFiles ? model.Roms?.ToResource() : null,
                Languages = model.Languages,
                Quality = model.Quality,
                CustomFormats = customFormats?.ToResource(false) ?? [],
                CustomFormatScore = customFormatScore,
                Size = model.Size,
                Title = model.Title,
                SizeLeft = model.SizeLeft,
                TimeLeft = model.TimeLeft,
                EstimatedCompletionTime = model.EstimatedCompletionTime,
                Added = model.Added,
                Status = model.Status,
                TrackedDownloadStatus = model.TrackedDownloadStatus,
                TrackedDownloadState = model.TrackedDownloadState,
                StatusMessages = model.StatusMessages,
                ErrorMessage = model.ErrorMessage,
                DownloadId = model.DownloadId,
                Protocol = model.Protocol,
                DownloadClient = model.DownloadClient,
                DownloadClientHasPostImportCategory = model.DownloadClientHasPostImportCategory,
                Indexer = model.Indexer,
                OutputPath = model.OutputPath,
                GameFilesWithFilesCount = model.Roms?.Count(e => e.HasFile) ?? 0,
                IsFullPlatform = model.RemoteRom?.ParsedRomInfo?.FullPlatform ?? false
            };
        }

        public static List<QueueResource> ToResource(this IEnumerable<Romarr.Core.Queue.Queue> models, bool includeSeries, bool includeGameFile)
        {
            return models.Select((m) => ToResource(m, includeSeries, includeGameFile)).ToList();
        }
    }
}
