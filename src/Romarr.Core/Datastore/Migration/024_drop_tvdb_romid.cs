using FluentMigrator;
using Romarr.Core.Datastore.Migration.Framework;

namespace Romarr.Core.Datastore.Migration
{
    [Migration(24)]
    public class drop_tvdb_episodeid : RomarrMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Delete.Column("TvDbEpisodeId").FromTable("Episodes");
        }
    }
}
