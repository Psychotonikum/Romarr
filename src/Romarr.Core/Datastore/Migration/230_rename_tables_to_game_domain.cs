using FluentMigrator;
using Romarr.Core.Datastore.Migration.Framework;

namespace Romarr.Core.Datastore.Migration
{
    [Migration(230)]
    public class rename_tables_to_game_domain : RomarrMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Rename.Table("Series").To("Games");
            Rename.Table("Episodes").To("Roms");
            Rename.Table("EpisodeFiles").To("RomFiles");
        }
    }
}
