using FluentMigrator;
using Romarr.Core.Datastore.Migration.Framework;

namespace Romarr.Core.Datastore.Migration
{
    [Migration(40)]
    public class add_metadata_to_episodes_and_series : RomarrMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("Series")
                 .AddColumn("Actors").AsString().Nullable()
                 .AddColumn("Ratings").AsString().Nullable()
                 .AddColumn("Genres").AsString().Nullable()
                 .AddColumn("Certification").AsString().Nullable();

            Alter.Table("Episodes")
                 .AddColumn("Ratings").AsString().Nullable()
                 .AddColumn("Images").AsString().Nullable();
        }
    }
}
