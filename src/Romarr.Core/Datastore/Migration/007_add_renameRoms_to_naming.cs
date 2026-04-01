using FluentMigrator;
using Romarr.Core.Datastore.Migration.Framework;

namespace Romarr.Core.Datastore.Migration
{
    [Migration(7)]
    public class add_renameRoms_to_naming : RomarrMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("NamingConfig")
                .AddColumn("RenameEpisodes")
                .AsBoolean()
                .Nullable();

            Execute.Sql("UPDATE \"NamingConfig\" SET \"RenameEpisodes\" = NOT \"UseSceneName\"");
        }
    }
}
