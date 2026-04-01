using FluentMigrator;
using Romarr.Core.Datastore.Migration.Framework;

namespace Romarr.Core.Datastore.Migration
{
    [Migration(168)]
    public class add_additional_info_to_pending_releases : RomarrMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("PendingReleases").AddColumn("AdditionalInfo").AsString().Nullable();
        }
    }
}
