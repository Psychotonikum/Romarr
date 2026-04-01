using FluentMigrator;
using Romarr.Core.Datastore.Migration.Framework;

namespace Romarr.Core.Datastore.Migration
{
    [Migration(11)]
    public class remove_ignored : RomarrMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Delete.Column("Ignored").FromTable("Platforms");
            Delete.Column("Ignored").FromTable("Episodes");
        }
    }
}
