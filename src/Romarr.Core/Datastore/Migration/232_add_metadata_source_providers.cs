using FluentMigrator;
using Romarr.Core.Datastore.Migration.Framework;

namespace Romarr.Core.Datastore.Migration
{
    [Migration(232)]
    public class add_metadata_source_providers : RomarrMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Create.TableForModel("MetadataSourceProviders")
                  .WithColumn("Enable").AsBoolean().NotNullable()
                  .WithColumn("Name").AsString().NotNullable()
                  .WithColumn("Implementation").AsString().NotNullable()
                  .WithColumn("Settings").AsString().Nullable()
                  .WithColumn("ConfigContract").AsString().Nullable()
                  .WithColumn("EnableSearch").AsBoolean().NotNullable().WithDefaultValue(true)
                  .WithColumn("EnableCalendar").AsBoolean().NotNullable().WithDefaultValue(true)
                  .WithColumn("DownloadMetadata").AsBoolean().NotNullable().WithDefaultValue(false)
                  .WithColumn("Tags").AsString().Nullable();
        }
    }
}
