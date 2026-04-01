using FluentMigrator;
using Romarr.Core.Datastore.Migration.Framework;

namespace Romarr.Core.Datastore.Migration
{
    [Migration(105)]
    public class rename_torrent_downloadstation : RomarrMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Update.Table("DownloadClients").Set(new { Implementation = "TorrentDownloadStation" }).Where(new { Implementation = "DownloadStation" });
        }
    }
}
