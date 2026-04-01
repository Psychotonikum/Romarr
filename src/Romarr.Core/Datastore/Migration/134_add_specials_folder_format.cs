using FluentMigrator;
using Romarr.Core.Datastore.Migration.Framework;

namespace Romarr.Core.Datastore.Migration
{
    [Migration(134)]
    public class add_specials_folder_format : RomarrMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("NamingConfig").AddColumn("SpecialsFolderFormat").AsString().Nullable();

            Update.Table("NamingConfig").Set(new { SpecialsFolderFormat = "Specials" }).AllRows();
        }
    }
}
