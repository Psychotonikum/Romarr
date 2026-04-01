using Romarr.Core.CustomFormats;
using Romarr.Core.History;
using Romarr.Core.Languages;
using Romarr.Core.Qualities;
using Romarr.Api.V5.CustomFormats;
using Romarr.Api.V5.Roms;
using Romarr.Api.V5.Game;
using Romarr.Http.REST;

namespace Romarr.Api.V5.History;

public class HistoryResource : RestResource
{
    public int FileId { get; set; }
    public int GameId { get; set; }
    public required string SourceTitle { get; set; }
    public required List<Language> Languages { get; set; }
    public required QualityModel Quality { get; set; }
    public required List<CustomFormatResource> CustomFormats { get; set; }
    public int CustomFormatScore { get; set; }
    public bool QualityCutoffNotMet { get; set; }
    public DateTime Date { get; set; }
    public string? DownloadId { get; set; }
    public FileHistoryEventType EventType { get; set; }
    public required Dictionary<string, string> Data { get; set; }
    public RomResource? Rom { get; set; }
    public GameResource? Game { get; set; }
}

public static class HistoryResourceMapper
{
    public static HistoryResource ToResource(this FileHistory model, ICustomFormatCalculationService formatCalculator)
    {
        var customFormats = formatCalculator.ParseCustomFormat(model, model.Game);
        var customFormatScore = model.Game.QualityProfile.Value.CalculateCustomFormatScore(customFormats);

        return new HistoryResource
        {
            Id = model.Id,
            FileId = model.FileId,
            GameId = model.GameId,
            SourceTitle = model.SourceTitle,
            Languages = model.Languages,
            Quality = model.Quality,
            CustomFormats = customFormats.ToResource(false),
            CustomFormatScore = customFormatScore,
            Date = model.Date,
            DownloadId = model.DownloadId,
            EventType = model.EventType,
            Data = model.Data
        };
    }
}
