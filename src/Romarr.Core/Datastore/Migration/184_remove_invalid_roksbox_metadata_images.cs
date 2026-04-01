using FluentMigrator;
using Romarr.Core.Datastore.Migration.Framework;

namespace Romarr.Core.Datastore.Migration
{
    [Migration(184)]
    public class remove_invalid_roksbox_metadata_images : RomarrMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            IfDatabase(ProcessorIdConstants.SQLite).Execute.Sql("DELETE FROM \"MetadataFiles\" WHERE \"Consumer\" = 'RoksboxMetadata' AND \"Type\" = 5 AND (\"RelativePath\" LIKE '%/metadata/%' OR \"RelativePath\" LIKE '%\\metadata\\%')");
            IfDatabase(ProcessorIdConstants.PostgreSQL).Execute.Sql("DELETE FROM \"MetadataFiles\" WHERE \"Consumer\" = 'RoksboxMetadata' AND \"Type\" = 5 AND (\"RelativePath\" LIKE '%/metadata/%' OR \"RelativePath\" LIKE '%\\\\metadata\\\\%')");
        }
    }
}
