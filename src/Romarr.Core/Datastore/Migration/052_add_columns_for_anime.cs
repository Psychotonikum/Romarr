using FluentMigrator;
using Romarr.Core.Datastore.Migration.Framework;

namespace Romarr.Core.Datastore.Migration
{
    [Migration(52)]
    public class add_columns_for_anime : RomarrMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            // Support XEM names
            Alter.Table("SceneMappings").AddColumn("Type").AsString().Nullable();
            Execute.Sql("DELETE FROM \"SceneMappings\"");

            // Add AnimeEpisodeFormat (set to Standard Rom format for now)
            Alter.Table("NamingConfig").AddColumn("AnimeEpisodeFormat").AsString().Nullable();
            Execute.Sql("UPDATE \"NamingConfig\" SET \"AnimeEpisodeFormat\" = \"StandardEpisodeFormat\"");
        }
    }
}
