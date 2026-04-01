using FluentMigrator;
using Romarr.Core.Datastore.Migration.Framework;

namespace Romarr.Core.Datastore.Migration
{
    [Migration(61)]
    public class clear_bad_scene_names : RomarrMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Execute.Sql("UPDATE \"EpisodeFiles\" " +
                        "SET \"ReleaseGroup\" = NULL , \"SceneName\" = NULL " +
                        "WHERE " +
                        "   \"ReleaseGroup\" IS NULL " +
                        "   OR \"SceneName\" IS NULL " +
                        "   OR \"ReleaseGroup\" = 'DRONE' " +
                        "   OR LENGTH(\"SceneName\") <10 " +
                        "   OR LENGTH(\"ReleaseGroup\") > 20 " +
                        "   OR \"SceneName\" NOT LIKE '%.%'");
        }
    }
}
