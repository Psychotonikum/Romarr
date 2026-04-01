using FluentMigrator;
using Romarr.Core.Datastore.Migration.Framework;

namespace Romarr.Core.Datastore.Migration
{
    [Migration(237)]
    public class rename_episode_columns_to_file : RomarrMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            // Roms table: rename episode-era columns
            Rename.Column("EpisodeNumber").OnTable("Roms").To("FileNumber");
            Rename.Column("EpisodeFileId").OnTable("Roms").To("RomFileId");
            Rename.Column("AbsoluteEpisodeNumber").OnTable("Roms").To("AbsoluteFileNumber");
            Rename.Column("SceneAbsoluteEpisodeNumber").OnTable("Roms").To("SceneAbsoluteFileNumber");
            Rename.Column("SceneEpisodeNumber").OnTable("Roms").To("SceneFileNumber");

            // History table: rename EpisodeId to FileId
            Rename.Column("EpisodeId").OnTable("History").To("FileId");

            // ExtraFiles table: rename EpisodeFileId to RomFileId
            if (Schema.Table("ExtraFiles").Column("EpisodeFileId").Exists())
            {
                Rename.Column("EpisodeFileId").OnTable("ExtraFiles").To("RomFileId");
            }

            // SubtitleFiles table: rename EpisodeFileId to RomFileId
            if (Schema.Table("SubtitleFiles").Column("EpisodeFileId").Exists())
            {
                Rename.Column("EpisodeFileId").OnTable("SubtitleFiles").To("RomFileId");
            }

            // MetadataFiles table: rename EpisodeFileId to RomFileId
            if (Schema.Table("MetadataFiles").Column("EpisodeFileId").Exists())
            {
                Rename.Column("EpisodeFileId").OnTable("MetadataFiles").To("RomFileId");
            }

            // NamingConfig table: rename episode-era format columns
            Rename.Column("RenameEpisodes").OnTable("NamingConfig").To("RenameGameFiles");
            Rename.Column("StandardEpisodeFormat").OnTable("NamingConfig").To("StandardGameFileFormat");
            Rename.Column("DailyEpisodeFormat").OnTable("NamingConfig").To("DailyGameFileFormat");
            Rename.Column("AnimeEpisodeFormat").OnTable("NamingConfig").To("AnimeGameFileFormat");
            Rename.Column("MultiEpisodeStyle").OnTable("NamingConfig").To("MultiGameFileStyle");

            // Games table: rename SeriesType to GameType
            Rename.Column("SeriesType").OnTable("Games").To("GameType");

            // ImportLists table: rename SeriesType to GameType
            if (Schema.Table("ImportLists").Column("SeriesType").Exists())
            {
                Rename.Column("SeriesType").OnTable("ImportLists").To("GameType");
            }

            // ImportLists table: rename SearchForMissingEpisodes to SearchForMissingGameFiles
            if (Schema.Table("ImportLists").Column("SearchForMissingEpisodes").Exists())
            {
                Rename.Column("SearchForMissingEpisodes").OnTable("ImportLists").To("SearchForMissingGameFiles");
            }

            // Indexers table: rename SeasonSearchMaximumSingleEpisodeAge
            if (Schema.Table("Indexers").Column("SeasonSearchMaximumSingleEpisodeAge").Exists())
            {
                Rename.Column("SeasonSearchMaximumSingleEpisodeAge").OnTable("Indexers").To("PlatformSearchMaximumSingleFileAge");
            }
        }
    }
}
