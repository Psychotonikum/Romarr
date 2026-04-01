using Romarr.Core.Languages;
using Romarr.Core.Parser.Model;
using Romarr.Core.Qualities;
using Romarr.Api.V5.CustomFormats;
using Romarr.Api.V5.Roms;
using Romarr.Http.REST;

namespace Romarr.Api.V5.ManualImport;

public class ManualImportReprocessResource : RestResource
{
    public string? Path { get; set; }
    public int GameId { get; set; }
    public int? PlatformNumber { get; set; }
    public List<RomResource> Roms { get; set; } = [];
    public List<int>? RomIds { get; set; }
    public QualityModel? Quality { get; set; }
    public List<Language> Languages { get; set; } = [];
    public string? ReleaseGroup { get; set; }
    public string? DownloadId { get; set; }
    public List<CustomFormatResource> CustomFormats { get; set; } = [];
    public int CustomFormatScore { get; set; }
    public int IndexerFlags { get; set; }
    public ReleaseType ReleaseType { get; set; }
    public IEnumerable<ImportRejectionResource> Rejections { get; set; } = [];
}
