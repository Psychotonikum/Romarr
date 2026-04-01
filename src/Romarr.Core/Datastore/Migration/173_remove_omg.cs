using FluentMigrator;
using Romarr.Core.Datastore.Migration.Framework;

namespace Romarr.Core.Datastore.Migration
{
    [Migration(173)]
    public class remove_omg : RomarrMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Execute.Sql("DELETE FROM \"Indexers\" WHERE \"Implementation\" = 'Omgwtfnzbs'");
        }
    }
}
