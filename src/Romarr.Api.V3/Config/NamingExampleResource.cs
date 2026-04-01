using Romarr.Core.Organizer;

namespace Romarr.Api.V3.Config
{
    public class NamingExampleResource
    {
        public string SingleGameFileExample { get; set; }
        public string MultiGameFileExample { get; set; }
        public string GameFolderExample { get; set; }
        public string PlatformFolderExample { get; set; }
    }

    public static class NamingConfigResourceMapper
    {
        public static NamingConfigResource ToResource(this NamingConfig model)
        {
            return new NamingConfigResource
            {
                Id = model.Id,

                RenameGameFiles = model.RenameGameFiles,
                ReplaceIllegalCharacters = model.ReplaceIllegalCharacters,
                ColonReplacementFormat = (int)model.ColonReplacementFormat,
                CustomColonReplacementFormat = model.CustomColonReplacementFormat,
                MultiGameFileStyle = (int)model.MultiGameFileStyle,
                StandardGameFileFormat = model.StandardGameFileFormat,
                GameFolderFormat = model.GameFolderFormat,
                PlatformFolderFormat = model.PlatformFolderFormat
            };
        }

        public static NamingConfig ToModel(this NamingConfigResource resource)
        {
            return new NamingConfig
            {
                Id = resource.Id,

                RenameGameFiles = resource.RenameGameFiles,
                ReplaceIllegalCharacters = resource.ReplaceIllegalCharacters,
                MultiGameFileStyle = (MultiGameFileStyle)resource.MultiGameFileStyle,
                ColonReplacementFormat = (ColonReplacementFormat)resource.ColonReplacementFormat,
                CustomColonReplacementFormat = resource.CustomColonReplacementFormat ?? "",
                StandardGameFileFormat = resource.StandardGameFileFormat,
                GameFolderFormat = resource.GameFolderFormat,
                PlatformFolderFormat = resource.PlatformFolderFormat
            };
        }
    }
}
