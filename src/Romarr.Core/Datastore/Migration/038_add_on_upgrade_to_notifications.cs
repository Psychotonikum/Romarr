using FluentMigrator;
using Romarr.Core.Datastore.Migration.Framework;

namespace Romarr.Core.Datastore.Migration
{
    [Migration(38)]
    public class add_on_upgrade_to_notifications : RomarrMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("Notifications").AddColumn("OnUpgrade").AsBoolean().Nullable();

            Execute.Sql("UPDATE \"Notifications\" SET \"OnUpgrade\" = \"OnDownload\"");
        }
    }
}
