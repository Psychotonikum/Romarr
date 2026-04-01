using FluentMigrator;
using Romarr.Core.Datastore.Migration.Framework;

namespace Romarr.Core.Datastore.Migration
{
    [Migration(165)]
    public class add_on_update_to_notifications : RomarrMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("Notifications").AddColumn("OnApplicationUpdate").AsBoolean().WithDefaultValue(false);
        }
    }
}
