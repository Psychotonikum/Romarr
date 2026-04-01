using FluentMigrator;
using Romarr.Core.Datastore.Migration.Framework;

namespace Romarr.Core.Datastore.Migration
{
    [Migration(14)]
    public class drop_air_date : RomarrMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Delete.Column("AirDate").FromTable("Episodes");
        }
    }
}
