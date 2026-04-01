using Romarr.Http.REST;

namespace Romarr.Api.V5.Settings;

public class NamingSettingsResource : RestResource
{
    public bool RenameRoms { get; set; }
    public bool ReplaceIllegalCharacters { get; set; }
    public int ColonReplacementFormat { get; set; }
    public string? CustomColonReplacementFormat { get; set; }
    public int MultiGameFileStyle { get; set; }
    public string? StandardGameFileFormat { get; set; }
    public string? GameFolderFormat { get; set; }
    public string? PlatformFolderFormat { get; set; }
}
