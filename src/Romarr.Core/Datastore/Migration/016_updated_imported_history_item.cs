using FluentMigrator;
using Romarr.Core.Datastore.Migration.Framework;

namespace Romarr.Core.Datastore.Migration
{
    [Migration(16)]
    public class updated_imported_history_item : RomarrMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Execute.Sql("UPDATE \"History\" SET \"Data\" = replace( \"Data\", '\"Path\"', '\"ImportedPath\"' ) WHERE \"EventType\" = 3");
        }
    }
}
