using FluentMigrator;
using Romarr.Core.Datastore.Migration.Framework;

namespace Romarr.Core.Datastore.Migration
{
    [Migration(17)]
    public class reset_scene_names : RomarrMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            // we were storing new file name as scene name.
            Execute.Sql("UPDATE \"EpisodeFiles\" SET \"SceneName\" = NULL where \"SceneName\" != NULL");
        }
    }
}
