using FluentMigrator;
using Romarr.Core.Datastore.Migration.Framework;

namespace Romarr.Core.Datastore.Migration
{
    [Migration(116)]
    public class disable_nyaa : RomarrMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Execute.Sql("UPDATE \"Indexers\" SET \"EnableRss\" = false, \"EnableSearch\" = false, \"Settings\" = Replace(\"Settings\", 'https://nyaa.se', '') WHERE \"Implementation\" = 'Nyaa' AND \"Settings\" LIKE '%nyaa.se%';");
        }
    }
}
