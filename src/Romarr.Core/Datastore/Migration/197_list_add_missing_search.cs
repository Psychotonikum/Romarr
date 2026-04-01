using FluentMigrator;
using Romarr.Core.Datastore.Migration.Framework;

namespace Romarr.Core.Datastore.Migration
{
    [Migration(197)]
    public class list_add_missing_search : RomarrMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("ImportLists").AddColumn("SearchForMissingEpisodes").AsBoolean().NotNullable().WithDefaultValue(true);
        }
    }
}
