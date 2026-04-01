using FluentMigrator;
using Romarr.Core.Datastore.Migration.Framework;

namespace Romarr.Core.Datastore.Migration
{
    [Migration(12)]
    public class remove_custom_start_date : RomarrMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Delete.Column("CustomStartDate").FromTable("Series");
        }
    }
}
