using System.Collections.Generic;
using System.Linq;
using NLog;
using Romarr.Common.Extensions;
using Romarr.Core.Download;
using Romarr.Core.Parser;
using Romarr.Core.Parser.Model;
using Romarr.Core.Games;

namespace Romarr.Core.MediaFiles.GameFileImport.Specifications
{
    public class MatchesFolderSpecification : IImportDecisionEngineSpecification
    {
        private readonly Logger _logger;
        private readonly IParsingService _parsingService;

        public MatchesFolderSpecification(IParsingService parsingService, Logger logger)
        {
            _logger = logger;
            _parsingService = parsingService;
        }

        public ImportSpecDecision IsSatisfiedBy(LocalGameFile localRom, DownloadClientItem downloadClientItem)
        {
            if (localRom.ExistingFile)
            {
                return ImportSpecDecision.Accept();
            }

            var fileInfo = localRom.FileRomInfo;
            var folderInfo = localRom.FolderRomInfo;

            if (fileInfo != null && fileInfo.IsPossibleScenePlatformSpecial)
            {
                fileInfo = _parsingService.ParseSpecialRomTitle(fileInfo, fileInfo.ReleaseTitle, localRom.Game.IgdbId, 0, null);
            }

            if (folderInfo != null && folderInfo.IsPossibleScenePlatformSpecial)
            {
                folderInfo = _parsingService.ParseSpecialRomTitle(folderInfo, folderInfo.ReleaseTitle, localRom.Game.IgdbId, 0, null);
            }

            if (folderInfo == null)
            {
                _logger.Debug("No folder ParsedRomInfo, skipping check");
                return ImportSpecDecision.Accept();
            }

            if (fileInfo == null)
            {
                _logger.Debug("No file ParsedRomInfo, skipping check");
                return ImportSpecDecision.Accept();
            }

            var folderGameFiles = _parsingService.GetRoms(folderInfo, localRom.Game, true);
            var fileGameFiles = _parsingService.GetRoms(fileInfo, localRom.Game, true);

            if (folderGameFiles.Empty())
            {
                _logger.Debug("No rom numbers in folder ParsedRomInfo, skipping check");
                return ImportSpecDecision.Accept();
            }

            var unexpected = fileGameFiles.Where(e => folderGameFiles.All(o => o.Id != e.Id)).ToList();

            if (unexpected.Any())
            {
                _logger.Debug("Unexpected rom(s) in file: {0}", FormatGameFile(unexpected));

                if (unexpected.Count == 1)
                {
                    return ImportSpecDecision.Reject(ImportRejectionReason.GameFileUnexpected, "Rom {0} was unexpected considering the {1} folder name", FormatGameFile(unexpected), folderInfo.ReleaseTitle);
                }

                return ImportSpecDecision.Reject(ImportRejectionReason.GameFileUnexpected, "Roms {0} were unexpected considering the {1} folder name", FormatGameFile(unexpected), folderInfo.ReleaseTitle);
            }

            return ImportSpecDecision.Accept();
        }

        private string FormatGameFile(List<Rom> roms)
        {
            return string.Join(", ", roms.Select(e => $"{e.PlatformNumber}x{e.FileNumber:00}"));
        }
    }
}
