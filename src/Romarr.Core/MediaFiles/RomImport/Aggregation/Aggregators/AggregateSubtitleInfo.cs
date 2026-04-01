using System;
using System.IO;
using System.Linq;
using NLog;
using Romarr.Common.Extensions;
using Romarr.Core.Download;
using Romarr.Core.Extras.Subtitles;
using Romarr.Core.Parser;
using Romarr.Core.Parser.Model;

namespace Romarr.Core.MediaFiles.GameFileImport.Aggregation.Aggregators
{
    public class AggregateSubtitleInfo : IAggregateLocalGameFile
    {
        public int Order => 2;

        private readonly Logger _logger;

        public AggregateSubtitleInfo(Logger logger)
        {
            _logger = logger;
        }

        public LocalGameFile Aggregate(LocalGameFile localRom, DownloadClientItem downloadClientItem)
        {
            var path = localRom.Path;
            var isSubtitleFile = SubtitleFileExtensions.Extensions.Contains(Path.GetExtension(path));

            if (!isSubtitleFile)
            {
                return localRom;
            }

            if (localRom.Roms.Empty())
            {
                return localRom;
            }

            var firstGameFile = localRom.Roms.First();
            var romFile = firstGameFile.RomFile.Value;
            localRom.SubtitleInfo = CleanSubtitleTitleInfo(romFile, path, localRom.FileNameBeforeRename);

            return localRom;
        }

        public SubtitleTitleInfo CleanSubtitleTitleInfo(RomFile romFile, string path, string fileNameBeforeRename)
        {
            var subtitleTitleInfo = LanguageParser.ParseSubtitleLanguageInformation(path);

            var romFileTitle = Path.GetFileNameWithoutExtension(fileNameBeforeRename ?? romFile.RelativePath);
            var originalRomFileTitle = Path.GetFileNameWithoutExtension(romFile.OriginalFilePath) ?? string.Empty;

            if (subtitleTitleInfo.TitleFirst && (romFileTitle.Contains(subtitleTitleInfo.RawTitle, StringComparison.OrdinalIgnoreCase) || originalRomFileTitle.Contains(subtitleTitleInfo.RawTitle, StringComparison.OrdinalIgnoreCase)))
            {
                _logger.Debug("Subtitle title '{0}' is in rom file title '{1}'. Removing from subtitle title.", subtitleTitleInfo.RawTitle, romFileTitle);

                subtitleTitleInfo = LanguageParser.ParseBasicSubtitle(path);
            }

            var cleanedTags = subtitleTitleInfo.LanguageTags.Where(t => !romFileTitle.Contains(t, StringComparison.OrdinalIgnoreCase)).ToList();

            if (cleanedTags.Count != subtitleTitleInfo.LanguageTags.Count)
            {
                _logger.Debug("Removed language tags '{0}' from subtitle title '{1}'.", string.Join(", ", subtitleTitleInfo.LanguageTags.Except(cleanedTags)), subtitleTitleInfo.RawTitle);
                subtitleTitleInfo.LanguageTags = cleanedTags;
            }

            return subtitleTitleInfo;
        }
    }
}
