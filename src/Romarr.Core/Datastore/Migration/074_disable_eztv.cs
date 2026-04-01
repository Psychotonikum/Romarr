using FluentMigrator;
using Romarr.Core.Datastore.Migration.Framework;

namespace Romarr.Core.Datastore.Migration
{
    [Migration(74)]
    public class disable_eztv : RomarrMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Execute.Sql("UPDATE \"Indexers\" SET \"EnableRss\" = false, \"EnableSearch\" = false WHERE \"Implementation\" = 'Eztv' AND \"Settings\" LIKE '%ezrss.it%'");
        }
    }
}
