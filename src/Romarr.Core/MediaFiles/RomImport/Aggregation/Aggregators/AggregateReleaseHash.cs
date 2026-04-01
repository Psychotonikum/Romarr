using Romarr.Common.Extensions;
using Romarr.Core.Download;
using Romarr.Core.Parser.Model;

namespace Romarr.Core.MediaFiles.GameFileImport.Aggregation.Aggregators
{
    public class AggregateReleaseHash : IAggregateLocalGameFile
    {
        public int Order => 1;

        public LocalGameFile Aggregate(LocalGameFile localRom, DownloadClientItem downloadClientItem)
        {
            var releaseHash = GetReleaseHash(localRom.FileRomInfo);

            if (releaseHash.IsNullOrWhiteSpace())
            {
                releaseHash = GetReleaseHash(localRom.DownloadClientRomInfo);
            }

            if (releaseHash.IsNullOrWhiteSpace())
            {
                releaseHash = GetReleaseHash(localRom.FolderRomInfo);
            }

            localRom.ReleaseHash = releaseHash;

            return localRom;
        }

        private string GetReleaseHash(ParsedRomInfo romInfo)
        {
            // ReleaseHash doesn't make sense for a FullPlatform, since hashes should be specific to a file
            if (romInfo == null || romInfo.FullPlatform)
            {
                return null;
            }

            return romInfo.ReleaseHash;
        }
    }
}
