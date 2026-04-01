using System;
using FluentMigrator;
using Romarr.Core.Datastore.Migration.Framework;

namespace Romarr.Core.Datastore.Migration
{
    [Migration(65)]
    public class make_scene_numbering_nullable : RomarrMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Update.Table("Episodes").Set(new { AbsoluteEpisodeNumber = DBNull.Value }).Where(new { AbsoluteEpisodeNumber = 0 });
            Update.Table("Episodes").Set(new { SceneAbsoluteEpisodeNumber = DBNull.Value }).Where(new { SceneAbsoluteEpisodeNumber = 0 });
            Update.Table("Episodes").Set(new { SceneSeasonNumber = DBNull.Value, SceneEpisodeNumber = DBNull.Value }).Where(new { SceneSeasonNumber = 0, SceneEpisodeNumber = 0 });
        }
    }
}
