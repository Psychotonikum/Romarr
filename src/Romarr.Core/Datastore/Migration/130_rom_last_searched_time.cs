using FluentMigrator;
using Romarr.Core.Datastore.Migration.Framework;

namespace Romarr.Core.Datastore.Migration
{
    [Migration(130)]
    public class episode_last_searched_time : RomarrMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("Episodes").AddColumn("LastSearchTime").AsDateTime().Nullable();
        }
    }
}
