using FluentMigrator;
using Romarr.Core.Datastore.Migration.Framework;

namespace Romarr.Core.Datastore.Migration
{
    [Migration(13)]
    public class add_air_date_utc : RomarrMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("Episodes").AddColumn("AirDateUtc").AsDateTime().Nullable();

            Execute.Sql("UPDATE \"Episodes\" SET \"AirDateUtc\" = \"AirDate\"");
        }
    }
}
