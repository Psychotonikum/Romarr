using Romarr.Core.Languages;
using Romarr.Core.Qualities;

namespace Romarr.Api.V5.Release;

public class ReleaseGrabResource
{
    public required string Guid { get; set; }
    public required int IndexerId { get; set; }
    public OverrideReleaseResource? Override { get; set; }
    public SearchInfoResource? SearchInfo { get; set; }
}

public class OverrideReleaseResource
{
    public int? GameId { get; set; }
    public List<int> RomIds { get; set; } = [];
    public int? DownloadClientId { get; set; }
    public QualityModel? Quality { get; set; }
    public List<Language> Languages { get; set; } = [];
}

public class SearchInfoResource
{
    public int? GameId { get; set; }
    public int? PlatformNumber { get; set; }
    public int? FileId { get; set; }
}
