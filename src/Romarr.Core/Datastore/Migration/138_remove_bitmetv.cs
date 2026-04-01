using FluentMigrator;
using Romarr.Core.Datastore.Migration.Framework;

namespace Romarr.Core.Datastore.Migration
{
    [Migration(138)]
    public class remove_bitmetv : RomarrMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Delete.FromTable("Indexers").Row(new { Implementation = "BitMeTv" });

            // Also disable usenet-crawler for the poor guys that still have it enabled
            Execute.Sql("UPDATE \"Indexers\" SET \"EnableRss\" = false, \"EnableAutomaticSearch\" = false, \"EnableInteractiveSearch\" = false WHERE \"Implementation\" = 'Newznab' AND \"Settings\" LIKE '%usenet-crawler.com%'");
        }
    }
}
