using Romarr.Core.Datastore;

namespace Romarr.Core.Organizer
{
    public class NamingConfig : ModelBase
    {
        public static NamingConfig Default => new NamingConfig
        {
            RenameGameFiles = false,
            ReplaceIllegalCharacters = true,
            ColonReplacementFormat = ColonReplacementFormat.Smart,
            CustomColonReplacementFormat = string.Empty,
            MultiGameFileStyle = MultiGameFileStyle.PrefixedRange,
            StandardGameFileFormat = "{Game Title} - {Platform} - {Rom Title} {Quality Full}",
            GameFolderFormat = "{Game Title}",
            PlatformFolderFormat = "Platform {platform}"
        };

        public bool RenameGameFiles { get; set; }
        public bool ReplaceIllegalCharacters { get; set; }
        public ColonReplacementFormat ColonReplacementFormat { get; set; }
        public string CustomColonReplacementFormat { get; set; }
        public MultiGameFileStyle MultiGameFileStyle { get; set; }
        public string StandardGameFileFormat { get; set; }
        public string GameFolderFormat { get; set; }
        public string PlatformFolderFormat { get; set; }
    }
}
