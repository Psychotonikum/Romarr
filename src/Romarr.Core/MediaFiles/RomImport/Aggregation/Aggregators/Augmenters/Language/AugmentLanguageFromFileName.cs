using System.Linq;
using Romarr.Core.Download;
using Romarr.Core.Parser;
using Romarr.Core.Parser.Model;

namespace Romarr.Core.MediaFiles.GameFileImport.Aggregation.Aggregators.Augmenters.Language
{
    public class AugmentLanguageFromFileName : IAugmentLanguage
    {
        public int Order => 1;
        public string Name => "FileName";

        public AugmentLanguageResult AugmentLanguage(LocalGameFile localRom, DownloadClientItem downloadClientItem)
        {
            var languages = localRom.FileRomInfo?.Languages;

            if (languages == null)
            {
                return null;
            }

            foreach (var rom in localRom.Roms)
            {
                var romTitleLanguage = LanguageParser.ParseLanguages(rom.Title);

                languages = languages.Except(romTitleLanguage).ToList();
            }

            return new AugmentLanguageResult(languages, Confidence.Filename);
        }
    }
}
