using FluentMigrator;
using Romarr.Core.Datastore.Migration.Framework;

namespace Romarr.Core.Datastore.Migration
{
    [Migration(127)]
    public class rename_restrictions_to_release_profiles : RomarrMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Rename.Table("Restrictions").To("ReleaseProfiles");
            Alter.Table("ReleaseProfiles").AddColumn("IncludePreferredWhenRenaming").AsBoolean().WithDefaultValue(true);
        }
    }
}
