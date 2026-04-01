using FluentMigrator;
using Romarr.Core.Datastore.Migration.Framework;

namespace Romarr.Core.Datastore.Migration
{
    [Migration(94)]
    public class add_tvmazeid : RomarrMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("Series").AddColumn("RawgId").AsInt32().WithDefaultValue(0);
            Create.Index().OnTable("Series").OnColumn("RawgId");
        }
    }
}
