using Dapper;
using Romarr.Core.Datastore;

namespace Romarr.Core.Housekeeping.Housekeepers
{
    public class CleanupOrphanedRomFiles : IHousekeepingTask
    {
        private readonly IMainDatabase _database;

        public CleanupOrphanedRomFiles(IMainDatabase database)
        {
            _database = database;
        }

        public void Clean()
        {
            using var mapper = _database.OpenConnection();
            mapper.Execute(@"DELETE FROM ""RomFiles""
                                     WHERE ""Id"" IN (
                                     SELECT ""RomFiles"".""Id"" FROM ""RomFiles""
                                     LEFT OUTER JOIN ""Roms""
                                     ON ""RomFiles"".""Id"" = ""Roms"".""RomFileId""
                                     WHERE ""Roms"".""Id"" IS NULL)");
        }
    }
}
