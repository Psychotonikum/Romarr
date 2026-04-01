using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Romarr.Common.Extensions;
using Romarr.Core.Extras.Metadata.Files;
using Romarr.Core.MediaFiles;
using Romarr.Core.Games;

namespace Romarr.Core.Extras.Metadata.Consumers.Plex
{
    public class PlexMetadata : MetadataBase<PlexMetadataSettings>
    {
        private readonly IRomService _romService;
        private readonly IMediaFileService _mediaFileService;

        public PlexMetadata(IRomService gameFileService, IMediaFileService mediaFileService)
        {
            _romService = gameFileService;
            _mediaFileService = mediaFileService;
        }

        public override string Name => "Plex";

        public override MetadataFile FindMetadataFile(Game game, string path)
        {
            var filename = Path.GetFileName(path);

            if (filename == null)
            {
                return null;
            }

            var relativePath = game.Path.GetRelativePath(path);

            if (relativePath == ".plexmatch")
            {
                return new MetadataFile
                {
                    GameId = game.Id,
                    Consumer = GetType().Name,
                    RelativePath = game.Path.GetRelativePath(path),
                    Type = MetadataType.SeriesMetadata
                };
            }

            return null;
        }

        public override MetadataFileResult SeriesMetadata(Game game, SeriesMetadataReason reason)
        {
            if (!Settings.SeriesPlexMatchFile)
            {
                return null;
            }

            var content = new StringBuilder();

            content.AppendLine($"Title: {game.Title}");
            content.AppendLine($"Year: {game.Year}");
            content.AppendLine($"IgdbId: {game.IgdbId}");
            content.AppendLine($"ImdbId: {game.ImdbId}");

            if (Settings.GameFileMappings)
            {
                var roms = _romService.GetGameFileBySeries(game.Id);
                var romFiles = _mediaFileService.GetFilesBySeries(game.Id);

                foreach (var romFile in romFiles)
                {
                    var gameFilesInFile = roms.Where(e => e.RomFileId == romFile.Id);
                    var gameFileFormat = $"S{romFile.PlatformNumber:00}{string.Join("-", gameFilesInFile.Select(e => $"E{e.FileNumber:00}"))}";

                    if (romFile.PlatformNumber == 0)
                    {
                        gameFileFormat = $"SP{gameFilesInFile.First().FileNumber:00}";
                    }

                    content.AppendLine($"Rom: {gameFileFormat}: {romFile.RelativePath}");
                }
            }

            return new MetadataFileResult(".plexmatch", content.ToString());
        }

        public override MetadataFileResult GameFileMetadata(Game game, RomFile romFile)
        {
            return null;
        }

        public override List<ImageFileResult> GameImages(Game game)
        {
            return new List<ImageFileResult>();
        }

        public override List<ImageFileResult> PlatformImages(Game game, Platform platform)
        {
            return new List<ImageFileResult>();
        }

        public override List<ImageFileResult> GameFileImages(Game game, RomFile romFile)
        {
            return new List<ImageFileResult>();
        }
    }
}
