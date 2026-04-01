using FluentMigrator;
using Romarr.Core.Datastore.Migration.Framework;

namespace Romarr.Core.Datastore.Migration
{
    [Migration(236)]
    public class add_rom_type_column : RomarrMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("Roms").AddColumn("RomType").AsInt32().WithDefaultValue(0);
        }
    }
}
