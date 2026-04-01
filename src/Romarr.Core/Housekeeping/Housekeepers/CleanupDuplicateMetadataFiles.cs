using Dapper;
using Romarr.Core.Datastore;

namespace Romarr.Core.Housekeeping.Housekeepers
{
    public class CleanupDuplicateMetadataFiles : IHousekeepingTask
    {
        private readonly IMainDatabase _database;

        public CleanupDuplicateMetadataFiles(IMainDatabase database)
        {
            _database = database;
        }

        public void Clean()
        {
            DeleteDuplicateSeriesMetadata();
            DeleteDuplicateGameFileMetadata();
            DeleteDuplicateGameFileImages();
        }

        private void DeleteDuplicateSeriesMetadata()
        {
            using var mapper = _database.OpenConnection();
            mapper.Execute(@"DELETE FROM ""MetadataFiles""
                                     WHERE ""Id"" IN (
                                         SELECT MIN(""Id"") FROM ""MetadataFiles""
                                         WHERE ""Type"" = 1
                                         GROUP BY ""GameId"", ""Consumer""
                                         HAVING COUNT(""GameId"") > 1
                                     )");
        }

        private void DeleteDuplicateGameFileMetadata()
        {
            using var mapper = _database.OpenConnection();
            mapper.Execute(@"DELETE FROM ""MetadataFiles""
                                     WHERE ""Id"" IN (
                                         SELECT MIN(""Id"") FROM ""MetadataFiles""
                                         WHERE ""Type"" = 2
                                         GROUP BY ""RomFileId"", ""Consumer""
                                         HAVING COUNT(""RomFileId"") > 1
                                     )");
        }

        private void DeleteDuplicateGameFileImages()
        {
            using var mapper = _database.OpenConnection();
            mapper.Execute(@"DELETE FROM ""MetadataFiles""
                                     WHERE ""Id"" IN (
                                         SELECT MIN(""Id"") FROM ""MetadataFiles""
                                         WHERE ""Type"" = 5
                                         GROUP BY ""RomFileId"", ""Consumer""
                                         HAVING COUNT(""RomFileId"") > 1
                                     )");
        }
    }
}
