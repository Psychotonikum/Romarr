using FluentMigrator;
using Romarr.Core.Datastore.Migration.Framework;

namespace Romarr.Core.Datastore.Migration
{
    [Migration(151)]
    public class remove_custom_filter_type : RomarrMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Update.Table("CustomFilters").Set(new { Type = "game" }).Where(new { Type = "seriesIndex" });
            Update.Table("CustomFilters").Set(new { Type = "game" }).Where(new { Type = "seriesEditor" });
            Update.Table("CustomFilters").Set(new { Type = "game" }).Where(new { Type = "platformPass" });
        }
    }
}
