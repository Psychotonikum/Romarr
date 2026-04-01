using System.Collections.Generic;
using System.IO;
using System.Linq;
using NLog;
using Romarr.Common.Extensions;
using Romarr.Core.Extras.Files;
using Romarr.Core.MediaFiles.GameFileImport.Aggregation;
using Romarr.Core.Parser.Model;
using Romarr.Core.Games;

namespace Romarr.Core.Extras.Others
{
    public class ExistingOtherExtraImporter : ImportExistingExtraFilesBase<OtherExtraFile>
    {
        private readonly IExtraFileService<OtherExtraFile> _otherExtraFileService;
        private readonly IAggregationService _aggregationService;
        private readonly Logger _logger;

        public ExistingOtherExtraImporter(IExtraFileService<OtherExtraFile> otherExtraFileService,
                                          IAggregationService aggregationService,
                                          Logger logger)
            : base(otherExtraFileService)
        {
            _otherExtraFileService = otherExtraFileService;
            _aggregationService = aggregationService;
            _logger = logger;
        }

        public override int Order => 2;

        public override IEnumerable<ExtraFile> ProcessFiles(Game game, List<string> filesOnDisk, List<string> importedFiles, string fileNameBeforeRename)
        {
            _logger.Debug("Looking for existing extra files in {0}", game.Path);

            var extraFiles = new List<OtherExtraFile>();
            var filterResult = FilterAndClean(game, filesOnDisk, importedFiles, fileNameBeforeRename is not null);

            foreach (var possibleExtraFile in filterResult.FilesOnDisk)
            {
                var extension = Path.GetExtension(possibleExtraFile);

                if (extension.IsNullOrWhiteSpace())
                {
                    _logger.Debug("No extension for file: {0}", possibleExtraFile);
                    continue;
                }

                var localRom = new LocalGameFile
                                   {
                                       FileRomInfo = Parser.Parser.ParsePath(possibleExtraFile),
                                       Game = game,
                                       Path = possibleExtraFile
                                   };

                try
                {
                    _aggregationService.Augment(localRom, null);
                }
                catch (AugmentingFailedException)
                {
                    _logger.Debug("Unable to parse extra file: {0}", possibleExtraFile);
                    continue;
                }

                if (localRom.Roms.Empty())
                {
                    _logger.Debug("Cannot find related roms for: {0}", possibleExtraFile);
                    continue;
                }

                if (localRom.Roms.DistinctBy(e => e.RomFileId).Count() > 1)
                {
                    _logger.Debug("Extra file: {0} does not match existing files.", possibleExtraFile);
                    continue;
                }

                var extraFile = new OtherExtraFile
                {
                    GameId = game.Id,
                    PlatformNumber = localRom.PlatformNumber,
                    RomFileId = localRom.Roms.First().RomFileId,
                    RelativePath = game.Path.GetRelativePath(possibleExtraFile),
                    Extension = extension
                };

                extraFiles.Add(extraFile);
            }

            _logger.Info("Found {0} existing other extra files", extraFiles.Count);
            _otherExtraFileService.Upsert(extraFiles);

            // Return files that were just imported along with files that were
            // previously imported so previously imported files aren't imported twice

            return extraFiles.Concat(filterResult.PreviouslyImported);
        }
    }
}
