using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using Romarr.Core.Datastore;
using Romarr.Core.MediaFiles;
using Romarr.Core.Games;

namespace Romarr.Core.GameStats
{
    public interface IGameStatisticsRepository
    {
        List<PlatformStatistics> GameStatistics();
        List<PlatformStatistics> GameStatistics(int gameId);
    }

    public class GameStatisticsRepository : IGameStatisticsRepository
    {
        private const string _selectGameFilesTemplate = "SELECT /**select**/ FROM \"Roms\" /**join**/ /**innerjoin**/ /**leftjoin**/ /**where**/ /**groupby**/ /**having**/ /**orderby**/";
        private const string _selectRomFilesTemplate = "SELECT /**select**/ FROM \"RomFiles\" /**join**/ /**innerjoin**/ /**leftjoin**/ /**where**/ /**groupby**/ /**having**/ /**orderby**/";

        private readonly IMainDatabase _database;

        public GameStatisticsRepository(IMainDatabase database)
        {
            _database = database;
        }

        public List<PlatformStatistics> GameStatistics()
        {
            var time = DateTime.UtcNow;
            return MapResults(Query(GameFilesBuilder(time), _selectGameFilesTemplate),
                Query(RomFilesBuilder(), _selectRomFilesTemplate));
        }

        public List<PlatformStatistics> GameStatistics(int gameId)
        {
            var time = DateTime.UtcNow;

            return MapResults(Query(GameFilesBuilder(time).Where<Rom>(x => x.GameId == gameId), _selectGameFilesTemplate),
                Query(RomFilesBuilder().Where<RomFile>(x => x.GameId == gameId), _selectRomFilesTemplate));
        }

        private List<PlatformStatistics> MapResults(List<PlatformStatistics> gameFilesResult, List<PlatformStatistics> filesResult)
        {
            gameFilesResult.ForEach(e =>
            {
                var file = filesResult.SingleOrDefault(f => f.GameId == e.GameId & f.PlatformNumber == e.PlatformNumber);

                e.SizeOnDisk = file?.SizeOnDisk ?? 0;
                e.ReleaseGroupsString = file?.ReleaseGroupsString;
            });

            return gameFilesResult;
        }

        private List<PlatformStatistics> Query(SqlBuilder builder, string template)
        {
            var sql = builder.AddTemplate(template).LogQuery();

            using (var conn = _database.OpenConnection())
            {
                return conn.Query<PlatformStatistics>(sql.RawSql, sql.Parameters).ToList();
            }
        }

        private SqlBuilder GameFilesBuilder(DateTime currentDate)
        {
            var parameters = new DynamicParameters();
            parameters.Add("currentDate", currentDate, null);

            var trueIndicator = _database.DatabaseType == DatabaseType.PostgreSQL ? "true" : "1";
            var falseIndicator = _database.DatabaseType == DatabaseType.PostgreSQL ? "false" : "0";

            return new SqlBuilder(_database.DatabaseType)
            .Select($@"""Roms"".""GameId"" AS GameId,
                             ""Roms"".""PlatformNumber"",
                             COUNT(*) AS TotalGameFileCount,
                             SUM(CASE WHEN ""AirDateUtc"" <= @currentDate OR ""RomFileId"" > 0 THEN 1 ELSE 0 END) AS AvailableGameFileCount,
                             SUM(CASE WHEN (""Monitored"" = {trueIndicator} AND ""AirDateUtc"" <= @currentDate) OR ""RomFileId"" > 0 THEN 1 ELSE 0 END) AS GameFileCount,
                             SUM(CASE WHEN ""RomFileId"" > 0 THEN 1 ELSE 0 END) AS RomFileCount,
                             SUM(CASE WHEN ""Monitored"" = {trueIndicator} THEN 1 ELSE 0 END) AS MonitoredGameFileCount,
                             MIN(CASE WHEN ""AirDateUtc"" < @currentDate OR ""Monitored"" = {falseIndicator} THEN NULL ELSE ""AirDateUtc"" END) AS NextAiringString,
                             MAX(CASE WHEN ""AirDateUtc"" >= @currentDate OR ""Monitored"" = {falseIndicator} THEN NULL ELSE ""AirDateUtc"" END) AS PreviousAiringString,
                             MAX(""AirDate"") AS LastAiredString",
                parameters)
            .GroupBy<Rom>(x => x.GameId)
            .GroupBy<Rom>(x => x.PlatformNumber);
        }

        private SqlBuilder RomFilesBuilder()
        {
            if (_database.DatabaseType == DatabaseType.SQLite)
            {
                return new SqlBuilder(_database.DatabaseType)
                .Select(@"""GameId"",
                            ""PlatformNumber"",
                            SUM(COALESCE(""Size"", 0)) AS SizeOnDisk,
                            GROUP_CONCAT(""ReleaseGroup"", '|') AS ReleaseGroupsString")
                .GroupBy<RomFile>(x => x.GameId)
                .GroupBy<RomFile>(x => x.PlatformNumber);
            }

            return new SqlBuilder(_database.DatabaseType)
                .Select(@"""GameId"",
                            ""PlatformNumber"",
                            SUM(COALESCE(""Size"", 0)) AS SizeOnDisk,
                            string_agg(""ReleaseGroup"", '|') AS ReleaseGroupsString")
                .GroupBy<RomFile>(x => x.GameId)
                .GroupBy<RomFile>(x => x.PlatformNumber);
        }
    }
}
