using Romarr.Core.Languages;
using Romarr.Core.Parser.Model;
using Romarr.Api.V5.CustomFormats;
using Romarr.Api.V5.Roms;
using Romarr.Api.V5.Game;
using Romarr.Http.REST;

namespace Romarr.Api.V5.Parse;

public class ParseResource : RestResource
{
    public string? Title { get; set; }
    public ParsedRomInfo? ParsedRomInfo { get; set; }
    public GameResource? Game { get; set; }
    public List<RomResource>? Roms { get; set; }
    public List<Language>? Languages { get; set; }
    public List<CustomFormatResource>? CustomFormats { get; set; }
    public int CustomFormatScore { get; set; }
}
