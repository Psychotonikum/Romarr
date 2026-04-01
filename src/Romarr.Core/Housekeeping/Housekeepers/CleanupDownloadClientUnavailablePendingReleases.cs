using System;
using Dapper;
using Romarr.Core.Datastore;
using Romarr.Core.Download.Pending;

namespace Romarr.Core.Housekeeping.Housekeepers
{
    public class CleanupDownloadClientUnavailablePendingReleases : IHousekeepingTask
    {
        private readonly IMainDatabase _database;

        public CleanupDownloadClientUnavailablePendingReleases(IMainDatabase database)
        {
            _database = database;
        }

        public void Clean()
        {
            using var mapper = _database.OpenConnection();

            if (_database.DatabaseType == DatabaseType.PostgreSQL)
            {
                mapper.Execute(@"DELETE FROM ""PendingReleases""
                            WHERE ""Added"" < @TwoWeeksAgo
                            AND ""Reason"" = ANY (@Reasons)",
                          new
                          {
                              TwoWeeksAgo = DateTime.UtcNow.AddDays(-14),
                              Reasons = new[] { (int)PendingReleaseReason.DownloadClientUnavailable, (int)PendingReleaseReason.Fallback }
                          });
            }
            else
            {
                mapper.Execute(@"DELETE FROM ""PendingReleases""
                            WHERE ""Added"" < @TwoWeeksAgo
                            AND ""REASON"" IN @Reasons",
                              new
                              {
                                  TwoWeeksAgo = DateTime.UtcNow.AddDays(-14),
                                  Reasons = new[] { (int)PendingReleaseReason.DownloadClientUnavailable, (int)PendingReleaseReason.Fallback }
                              });
            }
        }
    }
}
