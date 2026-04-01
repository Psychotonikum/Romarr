using FluentMigrator;
using Romarr.Core.Datastore.Migration.Framework;

namespace Romarr.Core.Datastore.Migration
{
    [Migration(129)]
    public class add_relative_original_path_to_episode_file : RomarrMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("EpisodeFiles").AddColumn("OriginalFilePath").AsString().Nullable();
        }
    }
}
