using FluentMigrator;
using Romarr.Core.Datastore.Migration.Framework;

namespace Romarr.Core.Datastore.Migration
{
    [Migration(54)]
    public class rename_profiles : RomarrMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Rename.Table("QualityProfiles").To("Profiles");

            Alter.Table("Profiles").AddColumn("Language").AsInt32().Nullable();
            Alter.Table("Profiles").AddColumn("GrabDelay").AsInt32().Nullable();
            Alter.Table("Profiles").AddColumn("GrabDelayMode").AsInt32().Nullable();
            Update.Table("Profiles").Set(new { Language = 1, GrabDelay = 0, GrabDelayMode = 0 }).AllRows();

            // Rename QualityProfileId in Game
            Alter.Table("Series").AddColumn("ProfileId").AsInt32().Nullable();
            Execute.Sql("UPDATE \"Series\" SET \"ProfileId\" = \"QualityProfileId\"");

            // Add HeldReleases
            Create.TableForModel("PendingReleases")
                  .WithColumn("SeriesId").AsInt32()
                  .WithColumn("Title").AsString()
                  .WithColumn("Added").AsDateTime()
                  .WithColumn("ParsedRomInfo").AsString()
                  .WithColumn("Release").AsString();
        }
    }
}
