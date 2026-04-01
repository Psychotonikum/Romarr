using FluentMigrator;
using Romarr.Core.Datastore.Migration.Framework;

namespace Romarr.Core.Datastore.Migration
{
    [Migration(177)]
    public class add_on_manual_interaction_required_to_notifications : RomarrMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("Notifications").AddColumn("OnManualInteractionRequired").AsBoolean().WithDefaultValue(false);
        }
    }
}
