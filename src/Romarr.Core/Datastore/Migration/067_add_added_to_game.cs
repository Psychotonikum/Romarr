using FluentMigrator;
using Romarr.Core.Datastore.Migration.Framework;

namespace Romarr.Core.Datastore.Migration
{
    [Migration(67)]
    public class add_added_to_series : RomarrMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("Series").AddColumn("Added").AsDateTime().Nullable();
        }
    }
}
