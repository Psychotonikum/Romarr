using Romarr.Core.Download;
using Romarr.Core.Games;
using Romarr.Core.Parser.Model;

namespace Romarr.Core.MediaFiles.GameFileImport.Aggregation.Aggregators
{
    public class AggregateRegion : IAggregateLocalGameFile
    {
        public int Order => 3;

        public LocalGameFile Aggregate(LocalGameFile localRom, DownloadClientItem downloadClientItem)
        {
            var fileName = System.IO.Path.GetFileName(localRom.Path);

            if (string.IsNullOrWhiteSpace(fileName))
            {
                return localRom;
            }

            var parsed = NoIntroFileNameParser.Parse(fileName);

            if (parsed == null)
            {
                return localRom;
            }

            localRom.Region = parsed.Region.ToString();
            localRom.Revision = parsed.Revision;
            localRom.DumpQuality = (int)parsed.DumpQuality;
            localRom.Modification = (int)parsed.Modification;
            localRom.ModificationName = parsed.ModificationName;
            localRom.RomReleaseType = (int)parsed.ReleaseType;

            return localRom;
        }
    }
}
