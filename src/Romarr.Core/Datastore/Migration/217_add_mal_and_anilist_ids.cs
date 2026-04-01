using FluentMigrator;
using Romarr.Core.Datastore.Migration.Framework;

namespace Romarr.Core.Datastore.Migration
{
    [Migration(217)]
    public class add_mal_and_anilist_ids : RomarrMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("Series").AddColumn("MalIds").AsString().WithDefaultValue("[]");
            Alter.Table("Series").AddColumn("AniListIds").AsString().WithDefaultValue("[]");
        }
    }
}
