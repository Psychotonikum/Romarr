using FluentMigrator;
using Romarr.Core.Datastore.Migration.Framework;

namespace Romarr.Core.Datastore.Migration
{
    [Migration(47)]
    public class add_temporary_blacklist_columns : RomarrMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("Blacklist").AddColumn("PublishedDate").AsDateTime().Nullable();
        }
    }
}
