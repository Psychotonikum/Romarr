using FluentMigrator;
using Romarr.Core.Datastore.Migration.Framework;

namespace Romarr.Core.Datastore.Migration
{
    [Migration(21)]
    public class drop_seasons_table : RomarrMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Delete.Table("Platforms");
        }
    }
}
