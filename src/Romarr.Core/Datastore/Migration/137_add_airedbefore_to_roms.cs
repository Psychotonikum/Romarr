using FluentMigrator;
using Romarr.Core.Datastore.Migration.Framework;

namespace Romarr.Core.Datastore.Migration
{
    [Migration(137)]
    public class add_airedbefore_to_episodes : RomarrMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("Episodes").AddColumn("AiredAfterPlatformNumber").AsInt32().Nullable()
                                   .AddColumn("AiredBeforePlatformNumber").AsInt32().Nullable()
                                   .AddColumn("AiredBeforeRomNumber").AsInt32().Nullable();
        }
    }
}
