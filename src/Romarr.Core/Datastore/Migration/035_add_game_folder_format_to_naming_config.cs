using FluentMigrator;
using Romarr.Core.Datastore.Migration.Framework;

namespace Romarr.Core.Datastore.Migration
{
    [Migration(35)]
    public class add_series_folder_format_to_naming_config : RomarrMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("NamingConfig").AddColumn("GameFolderFormat").AsString().Nullable();

            Update.Table("NamingConfig").Set(new { GameFolderFormat = "{Game Title}" }).AllRows();
        }
    }
}
