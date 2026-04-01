using FluentMigrator;
using Romarr.Core.Datastore.Migration.Framework;

namespace Romarr.Core.Datastore.Migration
{
    [Migration(23)]
    public class add_config_contract_to_indexers : RomarrMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Update.Table("Indexers").Set(new { ConfigContract = "NewznabSettings" }).Where(new { Implementation = "Newznab" });
            Update.Table("Indexers").Set(new { ConfigContract = "OmgwtfnzbsSettings" }).Where(new { Implementation = "Omgwtfnzbs" });
            Update.Table("Indexers").Set(new { ConfigContract = "NullConfig" }).Where(new { Implementation = "Wombles" });
            Update.Table("Indexers").Set(new { ConfigContract = "NullConfig" }).Where(new { Implementation = "Eztv" });

            Delete.FromTable("Indexers").IsNull("ConfigContract");
        }
    }
}
