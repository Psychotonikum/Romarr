using FluentMigrator;
using Romarr.Core.Datastore.Migration.Framework;

namespace Romarr.Core.Datastore.Migration
{
    [Migration(5)]
    public class added_eventtype_to_history : RomarrMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("History")
                .AddColumn("EventType")
                .AsInt32()
                .Nullable();
        }
    }
}
