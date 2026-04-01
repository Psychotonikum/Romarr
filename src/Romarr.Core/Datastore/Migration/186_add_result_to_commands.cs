using FluentMigrator;
using Romarr.Core.Datastore.Migration.Framework;

namespace Romarr.Core.Datastore.Migration
{
    [Migration(186)]
    public class add_result_to_commands : RomarrMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("Commands").AddColumn("Result").AsInt32().WithDefaultValue(1);
        }
    }
}
