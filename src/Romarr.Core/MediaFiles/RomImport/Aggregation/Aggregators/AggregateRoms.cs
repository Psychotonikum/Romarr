using System.Collections.Generic;
using System.IO;
using System.Linq;
using Romarr.Common.Extensions;
using Romarr.Core.Download;
using Romarr.Core.Parser;
using Romarr.Core.Parser.Model;
using Romarr.Core.Games;

namespace Romarr.Core.MediaFiles.GameFileImport.Aggregation.Aggregators
{
    public class AggregateFiles : IAggregateLocalGameFile
    {
        public int Order => 1;

        private readonly IParsingService _parsingService;
        private readonly IRomService _romService;

        public AggregateFiles(IParsingService parsingService, IRomService romService)
        {
            _parsingService = parsingService;
            _romService = romService;
        }

        public LocalGameFile Aggregate(LocalGameFile localRom, DownloadClientItem downloadClientItem)
        {
            localRom.Roms = GetRoms(localRom);

            return localRom;
        }

        private ParsedRomInfo GetBestRomInfo(LocalGameFile localRom)
        {
            var parsedRomInfo = localRom.FileRomInfo;
            var downloadClientRomInfo = localRom.DownloadClientRomInfo;
            var folderRomInfo = localRom.FolderRomInfo;

            if (!localRom.OtherVideoFiles && !SceneChecker.IsSceneTitle(Path.GetFileNameWithoutExtension(localRom.Path)))
            {
                if (downloadClientRomInfo != null &&
                    !downloadClientRomInfo.FullPlatform &&
                    PreferOtherRomInfo(parsedRomInfo, downloadClientRomInfo))
                {
                    parsedRomInfo = localRom.DownloadClientRomInfo;
                }
                else if (folderRomInfo != null &&
                         !folderRomInfo.FullPlatform &&
                         PreferOtherRomInfo(parsedRomInfo, folderRomInfo))
                {
                    parsedRomInfo = localRom.FolderRomInfo;
                }
            }

            if (parsedRomInfo == null)
            {
                parsedRomInfo = GetSpecialRomInfo(localRom, parsedRomInfo);
            }

            return parsedRomInfo;
        }

        private ParsedRomInfo GetSpecialRomInfo(LocalGameFile localRom, ParsedRomInfo parsedRomInfo)
        {
            var title = Path.GetFileNameWithoutExtension(localRom.Path);
            var specialRomInfo = _parsingService.ParseSpecialRomTitle(parsedRomInfo, title, localRom.Game);

            return specialRomInfo;
        }

        private List<Rom> GetRoms(LocalGameFile localRom)
        {
            var bestRomInfoForGameFiles = GetBestRomInfo(localRom);
            var isMediaFile = MediaFileExtensions.Extensions.Contains(Path.GetExtension(localRom.Path));

            if (bestRomInfoForGameFiles == null)
            {
                // Fallback: match ROM files by platform folder name
                return GetRomsByPlatformFolder(localRom);
            }

            if (ValidateParsedRomInfo.ValidateForGameType(bestRomInfoForGameFiles, localRom.Game, isMediaFile))
            {
                var roms = _parsingService.GetRoms(bestRomInfoForGameFiles, localRom.Game, localRom.SceneSource);

                if (roms.Empty() && bestRomInfoForGameFiles.IsPossibleSpecialGameFile)
                {
                    var parsedSpecialRomInfo = GetSpecialRomInfo(localRom, bestRomInfoForGameFiles);

                    if (parsedSpecialRomInfo != null)
                    {
                        roms = _parsingService.GetRoms(parsedSpecialRomInfo, localRom.Game, localRom.SceneSource);
                    }
                }

                // Fallback: if parsing found info but couldn't map to roms, try platform folder
                if (roms.Empty())
                {
                    roms = GetRomsByPlatformFolder(localRom);
                }

                return roms;
            }

            return new List<Rom>();
        }

        private List<Rom> GetRomsByPlatformFolder(LocalGameFile localRom)
        {
            var game = localRom.Game;

            if (game?.Platforms == null || !game.Platforms.Any())
            {
                return new List<Rom>();
            }

            var parentFolder = Path.GetDirectoryName(localRom.Path);
            var folderName = Path.GetFileName(parentFolder);

            if (folderName.IsNullOrWhiteSpace())
            {
                return new List<Rom>();
            }

            var matchedPlatform = game.Platforms
                .FirstOrDefault(p => p.Title != null &&
                    p.Title.Equals(folderName, System.StringComparison.OrdinalIgnoreCase));

            if (matchedPlatform == null)
            {
                return new List<Rom>();
            }

            return _romService.GetRomsByPlatform(game.Id, matchedPlatform.PlatformNumber);
        }

        private bool PreferOtherRomInfo(ParsedRomInfo fileRomInfo, ParsedRomInfo otherRomInfo)
        {
            if (fileRomInfo == null)
            {
                return true;
            }

            // When the files rom info is not absolute prefer it over a parsed rom info that is absolute
            if (!fileRomInfo.IsAbsoluteNumbering && otherRomInfo.IsAbsoluteNumbering)
            {
                return false;
            }

            return true;
        }
    }
}
