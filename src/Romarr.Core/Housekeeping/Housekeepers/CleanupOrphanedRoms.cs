using Dapper;
using Romarr.Core.Datastore;

namespace Romarr.Core.Housekeeping.Housekeepers
{
    public class CleanupOrphanedFiles : IHousekeepingTask
    {
        private readonly IMainDatabase _database;

        public CleanupOrphanedFiles(IMainDatabase database)
        {
            _database = database;
        }

        public void Clean()
        {
            using var mapper = _database.OpenConnection();
            mapper.Execute(@"DELETE FROM ""Roms""
                                     WHERE ""Id"" IN (
                                     SELECT ""Roms"".""Id"" FROM ""Roms""
                                     LEFT OUTER JOIN ""Games""
                                     ON ""Roms"".""GameId"" = ""Games"".""Id""
                                     WHERE ""Games"".""Id"" IS NULL)");
        }
    }
}
