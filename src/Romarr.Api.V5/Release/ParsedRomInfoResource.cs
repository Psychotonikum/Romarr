using Romarr.Core.Parser.Model;
using Romarr.Core.Qualities;

namespace Romarr.Api.V5.Release;

public class ParsedRomInfoResource
{
    public QualityModel? Quality { get; set; }
    public string? ReleaseGroup { get; set; }
    public string? ReleaseHash { get; set; }
    public bool FullPlatform { get; set; }
    public int PlatformNumber { get; set; }
    public string? AirDate { get; set; }
    public string? GameTitle { get; set; }
    public int[] RomNumbers { get; set; } = [];
    public int[] AbsoluteRomNumbers { get; set; } = [];
    public bool IsDaily { get; set; }
    public bool IsAbsoluteNumbering { get; set; }
    public bool IsPossibleSpecialGameFile { get; set; }
    public bool Special { get; set; }
}

public static class ParsedRomInfoResourceMapper
{
    public static ParsedRomInfoResource ToResource(this ParsedRomInfo parsedRomInfo)
    {
        return new ParsedRomInfoResource
        {
            Quality = parsedRomInfo.Quality,
            ReleaseGroup = parsedRomInfo.ReleaseGroup,
            ReleaseHash = parsedRomInfo.ReleaseHash,
            FullPlatform = parsedRomInfo.FullPlatform,
            PlatformNumber = parsedRomInfo.PlatformNumber,
            AirDate = parsedRomInfo.AirDate,
            GameTitle = parsedRomInfo.GameTitle,
            RomNumbers = parsedRomInfo.RomNumbers,
            AbsoluteRomNumbers = parsedRomInfo.AbsoluteRomNumbers,
            IsDaily = parsedRomInfo.IsDaily,
            IsAbsoluteNumbering = parsedRomInfo.IsAbsoluteNumbering,
            IsPossibleSpecialGameFile = parsedRomInfo.IsPossibleSpecialGameFile,
            Special = parsedRomInfo.Special,
        };
    }
}
