using FluentMigrator;
using Romarr.Core.Datastore.Migration.Framework;

namespace Romarr.Core.Datastore.Migration
{
    [Migration(60)]
    public class remove_enable_from_indexers : RomarrMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Delete.Column("Enable").FromTable("Indexers");
            Delete.Column("Protocol").FromTable("DownloadClients");
        }
    }
}
