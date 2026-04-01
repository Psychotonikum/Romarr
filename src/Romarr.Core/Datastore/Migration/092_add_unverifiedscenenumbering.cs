using FluentMigrator;
using Romarr.Core.Datastore.Migration.Framework;

namespace Romarr.Core.Datastore.Migration
{
    [Migration(92)]
    public class add_unverifiedscenenumbering : RomarrMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("Episodes").AddColumn("UnverifiedSceneNumbering").AsBoolean().WithDefaultValue(false);
        }
    }
}
