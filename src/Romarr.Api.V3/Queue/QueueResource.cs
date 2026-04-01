using System;
using System.Collections.Generic;
using System.Linq;
using Romarr.Core.Download.TrackedDownloads;
using Romarr.Core.Indexers;
using Romarr.Core.Languages;
using Romarr.Core.Qualities;
using Romarr.Core.Queue;
using Romarr.Api.V3.CustomFormats;
using Romarr.Api.V3.Roms;
using Romarr.Api.V3.Game;
using Romarr.Http.REST;

#pragma warning disable CS0612
namespace Romarr.Api.V3.Queue
{
    public class QueueResource : RestResource
    {
        public int? GameId { get; set; }
        public int? FileId { get; set; }
        public int? PlatformNumber { get; set; }
        public GameResource Game { get; set; }
        public RomResource Rom { get; set; }
        public List<Language> Languages { get; set; }
        public QualityModel Quality { get; set; }
        public List<CustomFormatResource> CustomFormats { get; set; }
        public int CustomFormatScore { get; set; }
        public decimal Size { get; set; }
        public string Title { get; set; }

        // Collides with existing properties due to case-insensitive deserialization
        // public decimal SizeLeft { get; set; }
        // public TimeSpan? TimeLeft { get; set; }

        public DateTime? EstimatedCompletionTime { get; set; }
        public DateTime? Added { get; set; }
        public QueueStatus Status { get; set; }
        public TrackedDownloadStatus? TrackedDownloadStatus { get; set; }
        public TrackedDownloadState? TrackedDownloadState { get; set; }
        public List<TrackedDownloadStatusMessage> StatusMessages { get; set; }
        public string ErrorMessage { get; set; }
        public string DownloadId { get; set; }
        public DownloadProtocol Protocol { get; set; }
        public string DownloadClient { get; set; }
        public bool DownloadClientHasPostImportCategory { get; set; }
        public string Indexer { get; set; }
        public string OutputPath { get; set; }
        public bool GameFileHasFile { get; set; }

        [Obsolete("Will be replaced by SizeLeft")]
        public decimal Sizeleft { get; set; }

        [Obsolete("Will be replaced by TimeLeft")]
        public TimeSpan? Timeleft { get; set; }
    }

    public static class QueueResourceMapper
    {
        public static QueueResource ToResource(this Romarr.Core.Queue.Queue model, bool includeSeries, bool includeGameFile)
        {
            if (model == null)
            {
                return null;
            }

            var customFormats = model.RemoteRom?.CustomFormats;
            var customFormatScore = model.Game?.QualityProfile?.Value?.CalculateCustomFormatScore(customFormats) ?? 0;

            return new QueueResource
            {
                Id = model.Id,
                GameId = model.Game?.Id,
                FileId = model.Rom?.Id,
                PlatformNumber = model.Rom?.PlatformNumber,
                Game = includeSeries && model.Game != null ? model.Game.ToResource() : null,
                Rom = includeGameFile && model.Rom != null ? model.Rom.ToResource() : null,
                Languages = model.Languages,
                Quality = model.Quality,
                CustomFormats = customFormats?.ToResource(false),
                CustomFormatScore = customFormatScore,
                Size = model.Size,
                Title = model.Title,

                // Collides with existing properties due to case-insensitive deserialization
                // SizeLeft = model.SizeLeft,
                // TimeLeft = model.TimeLeft,

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
                GameFileHasFile = model.Rom?.HasFile ?? false,

                #pragma warning disable CS0618
                Sizeleft = model.SizeLeft,
                Timeleft = model.TimeLeft,
                #pragma warning restore CS0618
            };
        }

        public static List<QueueResource> ToResource(this IEnumerable<Romarr.Core.Queue.Queue> models, bool includeSeries, bool includeGameFile)
        {
            return models.Select((m) => ToResource(m, includeSeries, includeGameFile)).ToList();
        }
    }
}
#pragma warning restore CS0612
