using Romarr.Http.REST;

namespace Romarr.Api.V3.Config
{
    public class NamingConfigResource : RestResource
    {
        public bool RenameGameFiles { get; set; }
        public bool ReplaceIllegalCharacters { get; set; }
        public int ColonReplacementFormat { get; set; }
        public string CustomColonReplacementFormat { get; set; }
        public int MultiGameFileStyle { get; set; }
        public string StandardGameFileFormat { get; set; }
        public string GameFolderFormat { get; set; }
        public string PlatformFolderFormat { get; set; }
    }
}
