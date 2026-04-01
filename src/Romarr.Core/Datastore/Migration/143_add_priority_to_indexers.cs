using FluentMigrator;
using Romarr.Core.Datastore.Migration.Framework;

namespace Romarr.Core.Datastore.Migration
{
    [Migration(143)]
    public class add_priority_to_indexers : RomarrMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("Indexers").AddColumn("Priority").AsInt32().NotNullable().WithDefaultValue(25);
        }
    }
}
