using FluentMigrator;
using Romarr.Core.Datastore.Migration.Framework;

namespace Romarr.Core.Datastore.Migration;

[Migration(226)]
public class add_air_date_filtering_to_release_profiles : RomarrMigrationBase
{
    protected override void MainDbUpgrade()
    {
        Alter.Table("ReleaseProfiles").AddColumn("AirDateRestriction").AsBoolean().WithDefaultValue(false);
        Alter.Table("ReleaseProfiles").AddColumn("AirDateGracePeriod").AsInt32().WithDefaultValue(0);
    }
}
