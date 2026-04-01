using FluentMigrator;
using Romarr.Core.Datastore.Migration.Framework;

namespace Romarr.Core.Datastore.Migration
{
    [Migration(44)]
    public class fix_xbmc_episode_metadata : RomarrMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            // Convert Rom Metadata to proper type
            Execute.Sql("UPDATE \"MetadataFiles\" " +
                        "SET \"Type\" = 2 " +
                        "WHERE \"Consumer\" = 'XbmcMetadata' " +
                        "AND \"EpisodeFileId\" IS NOT NULL " +
                        "AND \"Type\" = 4 " +
                        "AND \"RelativePath\" LIKE '%.nfo'");

            // Convert Rom Images to proper type
            Execute.Sql("UPDATE \"MetadataFiles\" " +
                        "SET \"Type\" = 5 " +
                        "WHERE \"Consumer\" = 'XbmcMetadata' " +
                        "AND \"EpisodeFileId\" IS NOT NULL " +
                        "AND \"Type\" = 4");
        }
    }
}
