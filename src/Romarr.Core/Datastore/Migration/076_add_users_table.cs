using FluentMigrator;
using Romarr.Core.Datastore.Migration.Framework;

namespace Romarr.Core.Datastore.Migration
{
    [Migration(76)]
    public class add_users_table : RomarrMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Create.TableForModel("Users")
                  .WithColumn("Identifier").AsString().NotNullable().Unique()
                  .WithColumn("Username").AsString().NotNullable().Unique()
                  .WithColumn("Password").AsString().NotNullable();
        }
    }
}
