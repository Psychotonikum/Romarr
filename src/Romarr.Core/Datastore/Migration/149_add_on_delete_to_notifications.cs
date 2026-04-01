using FluentMigrator;
using Romarr.Core.Datastore.Migration.Framework;

namespace Romarr.Core.Datastore.Migration
{
    [Migration(149)]
    public class add_on_delete_to_notifications : RomarrMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("Notifications").AddColumn("OnSeriesDelete").AsBoolean().WithDefaultValue(false);
            Alter.Table("Notifications").AddColumn("OnRomFileDelete").AsBoolean().WithDefaultValue(false);
        }
    }
}
