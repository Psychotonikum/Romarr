using FluentMigrator;
using Romarr.Core.Datastore.Migration.Framework;

namespace Romarr.Core.Datastore.Migration
{
    [Migration(205)]
    public class rename_season_pack_spec : RomarrMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Execute.Sql("UPDATE \"CustomFormats\" SET \"Specifications\" = REPLACE(\"Specifications\", 'SeasonPackSpecification', 'ReleaseTypeSpecification')");
        }
    }
}
