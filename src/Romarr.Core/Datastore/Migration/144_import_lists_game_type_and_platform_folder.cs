using FluentMigrator;
using Romarr.Core.Datastore.Migration.Framework;

namespace Romarr.Core.Datastore.Migration
{
    [Migration(144)]
    public class import_lists_series_type_and_season_folder : RomarrMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("ImportLists").AddColumn("SeriesType").AsInt32().WithDefaultValue(0);
            Alter.Table("ImportLists").AddColumn("PlatformFolder").AsBoolean().WithDefaultValue(true);
        }
    }
}
