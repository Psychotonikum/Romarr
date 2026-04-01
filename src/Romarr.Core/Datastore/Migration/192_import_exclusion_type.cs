using FluentMigrator;
using Romarr.Core.Datastore.Migration.Framework;

namespace Romarr.Core.Datastore.Migration
{
    [Migration(192)]
    public class import_exclusion_type : RomarrMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            IfDatabase(ProcessorIdConstants.SQLite).Alter.Table("ImportListExclusions").AlterColumn("TvdbId").AsInt32();

            // PG cannot autocast varchar to integer
            IfDatabase(ProcessorIdConstants.PostgreSQL).Execute.Sql("ALTER TABLE \"ImportListExclusions\" ALTER COLUMN \"TvdbId\" TYPE INTEGER USING \"TvdbId\"::integer");
        }
    }
}
