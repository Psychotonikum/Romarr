using FluentMigrator;
using Romarr.Core.Datastore.Migration.Framework;

namespace Romarr.Core.Datastore.Migration
{
    [Migration(206)]
    public class add_tmdbid : RomarrMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("Series").AddColumn("TmdbId").AsInt32().WithDefaultValue(0);
            Create.Index().OnTable("Series").OnColumn("TmdbId");
        }
    }
}
