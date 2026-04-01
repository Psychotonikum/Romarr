using System;
using FluentMigrator;
using Romarr.Core.Datastore.Migration.Framework;

namespace Romarr.Core.Datastore.Migration
{
    [Migration(32)]
    public class set_default_release_group : RomarrMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Update.Table("EpisodeFiles").Set(new { ReleaseGroup = "DRONE" }).Where(new { ReleaseGroup = DBNull.Value });
        }
    }
}
