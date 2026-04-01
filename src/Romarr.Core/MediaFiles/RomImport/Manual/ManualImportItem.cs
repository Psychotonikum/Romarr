using System.Collections.Generic;
using Romarr.Core.CustomFormats;
using Romarr.Core.Languages;
using Romarr.Core.Parser.Model;
using Romarr.Core.Qualities;
using Romarr.Core.Games;

namespace Romarr.Core.MediaFiles.GameFileImport.Manual
{
    public class ManualImportItem
    {
        public string Path { get; set; }
        public string RelativePath { get; set; }
        public string FolderName { get; set; }
        public string Name { get; set; }
        public long Size { get; set; }
        public Game Game { get; set; }
        public int? PlatformNumber { get; set; }
        public List<Rom> Roms { get; set; }
        public int? RomFileId { get; set; }
        public QualityModel Quality { get; set; } = new();
        public List<Language> Languages { get; set; } = new();
        public string ReleaseGroup { get; set; }
        public string DownloadId { get; set; }
        public List<CustomFormat> CustomFormats { get; set; } = new();
        public int CustomFormatScore { get; set; }
        public int IndexerFlags { get; set; }
        public ReleaseType ReleaseType { get; set; }
        public IEnumerable<ImportRejection> Rejections { get; set; } = new List<ImportRejection>();
    }
}
