using System.Collections.Generic;
using System.Linq;
using Romarr.Common.Extensions;
using Romarr.Core.Download;
using Romarr.Core.Parser;
using Romarr.Core.Parser.Model;

namespace Romarr.Core.MediaFiles.GameFileImport.Aggregation.Aggregators.Augmenters.Language
{
    public class AugmentLanguageFromMediaInfo : IAugmentLanguage
    {
        public int Order => 4;
        public string Name => "MediaInfo";

        public AugmentLanguageResult AugmentLanguage(LocalGameFile localRom, DownloadClientItem downloadClientItem)
        {
            if (localRom.MediaInfo == null)
            {
                return null;
            }

            var audioLanguages = localRom.MediaInfo.AudioStreams?.Select(l => l.Language).Distinct().ToList() ?? [];

            var languages = new List<Languages.Language>();

            foreach (var audioLanguage in audioLanguages)
            {
                var language = IsoLanguages.Find(audioLanguage)?.Language;
                languages.AddIfNotNull(language);
            }

            if (languages.Count == 0)
            {
                return null;
            }

            return new AugmentLanguageResult(languages, Confidence.MediaInfo);
        }
    }
}
