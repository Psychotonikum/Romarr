using FluentMigrator;
using Romarr.Core.Datastore.Migration.Framework;

namespace Romarr.Core.Datastore.Migration
{
    [Migration(45)]
    public class add_indexes : RomarrMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Create.Index().OnTable("Blacklist").OnColumn("SeriesId");

            Create.Index().OnTable("EpisodeFiles").OnColumn("SeriesId");

            Create.Index().OnTable("Episodes").OnColumn("EpisodeFileId");
            Create.Index().OnTable("Episodes").OnColumn("SeriesId");

            Create.Index().OnTable("History").OnColumn("EpisodeId");
            Create.Index().OnTable("History").OnColumn("Date");

            Create.Index().OnTable("Series").OnColumn("Path");
            Create.Index().OnTable("Series").OnColumn("CleanTitle");
            Create.Index().OnTable("Series").OnColumn("MobyGamesId");
        }
    }
}
