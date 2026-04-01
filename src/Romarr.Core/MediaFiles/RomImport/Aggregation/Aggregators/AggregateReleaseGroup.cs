using Romarr.Common.Extensions;
using Romarr.Core.Download;
using Romarr.Core.Parser.Model;

namespace Romarr.Core.MediaFiles.GameFileImport.Aggregation.Aggregators
{
    public class AggregateReleaseGroup : IAggregateLocalGameFile
    {
        public int Order => 1;

        public LocalGameFile Aggregate(LocalGameFile localRom, DownloadClientItem downloadClientItem)
        {
            // Prefer ReleaseGroup from DownloadClient/Folder if they're not a platform pack
            var releaseGroup = GetReleaseGroup(localRom.DownloadClientRomInfo, true);

            if (releaseGroup.IsNullOrWhiteSpace())
            {
                releaseGroup = GetReleaseGroup(localRom.FolderRomInfo, true);
            }

            if (releaseGroup.IsNullOrWhiteSpace())
            {
                releaseGroup = GetReleaseGroup(localRom.FileRomInfo, false);
            }

            if (releaseGroup.IsNullOrWhiteSpace())
            {
                releaseGroup = GetReleaseGroup(localRom.DownloadClientRomInfo, false);
            }

            if (releaseGroup.IsNullOrWhiteSpace())
            {
                releaseGroup = GetReleaseGroup(localRom.FolderRomInfo, false);
            }

            localRom.ReleaseGroup = releaseGroup;

            return localRom;
        }

        private string GetReleaseGroup(ParsedRomInfo romInfo, bool skipFullPlatform)
        {
            if (romInfo == null || (romInfo.FullPlatform && skipFullPlatform))
            {
                return null;
            }

            return romInfo.ReleaseGroup;
        }
    }
}
