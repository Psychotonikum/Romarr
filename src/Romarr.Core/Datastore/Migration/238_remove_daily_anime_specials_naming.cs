using FluentMigrator;
using Romarr.Core.Datastore.Migration.Framework;

namespace Romarr.Core.Datastore.Migration
{
    [Migration(238)]
    public class remove_daily_anime_specials_naming : RomarrMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Delete.Column("DailyGameFileFormat").FromTable("NamingConfig");
            Delete.Column("AnimeGameFileFormat").FromTable("NamingConfig");
            Delete.Column("SpecialsFolderFormat").FromTable("NamingConfig");
        }
    }
}
