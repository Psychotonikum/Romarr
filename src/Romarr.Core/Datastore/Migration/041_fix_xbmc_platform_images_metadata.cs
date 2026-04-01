using FluentMigrator;
using Romarr.Core.Datastore.Migration.Framework;

namespace Romarr.Core.Datastore.Migration
{
    [Migration(41)]
    public class fix_xbmc_season_images_metadata : RomarrMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Execute.Sql("UPDATE \"MetadataFiles\" SET \"Type\" = 4 WHERE \"Consumer\" = 'XbmcMetadata' AND \"SeasonNumber\" IS NOT NULL");
        }
    }
}
