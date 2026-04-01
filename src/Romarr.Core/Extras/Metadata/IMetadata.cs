using System.Collections.Generic;
using Romarr.Core.Extras.Metadata.Files;
using Romarr.Core.MediaFiles;
using Romarr.Core.ThingiProvider;
using Romarr.Core.Games;

namespace Romarr.Core.Extras.Metadata
{
    public interface IMetadata : IProvider
    {
        string GetFilenameAfterMove(Game game, RomFile romFile, MetadataFile metadataFile);
        MetadataFile FindMetadataFile(Game game, string path);
        MetadataFileResult SeriesMetadata(Game game, SeriesMetadataReason reason);
        MetadataFileResult GameFileMetadata(Game game, RomFile romFile);
        List<ImageFileResult> GameImages(Game game);
        List<ImageFileResult> PlatformImages(Game game, Platform platform);
        List<ImageFileResult> GameFileImages(Game game, RomFile romFile);
    }

    public enum SeriesMetadataReason
    {
        Scan,
        GameFileFolderCreated,
        GameFilesImported
    }
}
