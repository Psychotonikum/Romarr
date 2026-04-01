using System.Linq;
using Romarr.Core.Download;
using Romarr.Core.Parser;
using Romarr.Core.Parser.Model;

namespace Romarr.Core.MediaFiles.GameFileImport.Aggregation.Aggregators.Augmenters.Language
{
    public class AugmentLanguageFromFolder : IAugmentLanguage
    {
        public int Order => 2;
        public string Name => "FolderName";

        public AugmentLanguageResult AugmentLanguage(LocalGameFile localRom, DownloadClientItem downloadClientItem)
        {
            var languages = localRom.FolderRomInfo?.Languages;

            if (languages == null)
            {
                return null;
            }

            foreach (var rom in localRom.Roms)
            {
                var romTitleLanguage = LanguageParser.ParseLanguages(rom.Title);

                languages = languages.Except(romTitleLanguage).ToList();
            }

            return new AugmentLanguageResult(languages, Confidence.Foldername);
        }
    }
}
