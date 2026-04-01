using FluentMigrator;
using Romarr.Core.Datastore.Migration.Framework;

namespace Romarr.Core.Datastore.Migration
{
    [Migration(153)]
    public class add_on_episodefiledelete_for_upgrade : RomarrMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("Notifications").AddColumn("OnRomFileDeleteForUpgrade").AsBoolean().WithDefaultValue(true);
        }
    }
}
