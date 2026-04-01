using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using NLog;
using Romarr.Core.Datastore;
using Romarr.Core.MediaFiles;
using Romarr.Core.Messaging.Events;
using Romarr.Core.Qualities;

namespace Romarr.Core.Games
{
    public interface IRomRepository : IBasicRepository<Rom>
    {
        Rom Find(int gameId, int platform, int romNumber);
        Rom Find(int gameId, int absoluteRomNumber);
        List<Rom> Find(int gameId, string date);
        List<Rom> GetRoms(int gameId);
        List<Rom> GetRoms(int gameId, int platformNumber);
        List<Rom> GetRomsByGameIds(List<int> gameIds);
        List<Rom> GetRomsByScenePlatform(int gameId, int scenePlatformNumber);
        List<Rom> GetGameFileByFileId(int fileId);
        List<Rom> GameFilesWithFiles(int gameId);
        PagingSpec<Rom> GameFilesWithoutFiles(PagingSpec<Rom> pagingSpec, bool includeSpecials);
        PagingSpec<Rom> GameFilesWhereCutoffUnmet(PagingSpec<Rom> pagingSpec, List<QualitiesBelowCutoff> qualitiesBelowCutoff, bool includeSpecials);
        List<Rom> FindGameFilesBySceneNumbering(int gameId, int platformNumber, int romNumber);
        List<Rom> FindGameFilesBySceneNumbering(int gameId, int sceneAbsoluteRomNumber);
        List<Rom> GameFilesBetweenDates(DateTime startDate, DateTime endDate, bool includeUnmonitored, bool includeSpecials);
        void SetMonitoredFlat(Rom rom, bool monitored);
        void SetMonitoredByPlatform(int gameId, int platformNumber, bool monitored);
        void SetMonitored(IEnumerable<int> ids, bool monitored);
        void SetFileId(Rom rom, int fileId);
        void ClearFileId(Rom rom, bool unmonitor);
    }

    public class RomRepository : BasicRepository<Rom>, IRomRepository
    {
        private readonly Logger _logger;

        public RomRepository(IMainDatabase database, IEventAggregator eventAggregator, Logger logger)
            : base(database, eventAggregator)
        {
            _logger = logger;
        }

        protected override IEnumerable<Rom> PagedQuery(SqlBuilder builder) =>
            _database.QueryJoined<Rom, Game>(builder, (rom, game) =>
            {
                rom.Game = game;
                return rom;
            });

        public Rom Find(int gameId, int platform, int romNumber)
        {
            return Query(s => s.GameId == gameId && s.PlatformNumber == platform && s.FileNumber == romNumber)
                               .SingleOrDefault();
        }

        public Rom Find(int gameId, int absoluteRomNumber)
        {
            return Query(s => s.GameId == gameId && s.AbsoluteFileNumber == absoluteRomNumber)
                        .SingleOrDefault();
        }

        public List<Rom> Find(int gameId, string date)
        {
            return Query(s => s.GameId == gameId && s.AirDate == date).ToList();
        }

        public List<Rom> GetRoms(int gameId)
        {
            return Query(s => s.GameId == gameId).ToList();
        }

        public List<Rom> GetRoms(int gameId, int platformNumber)
        {
            return Query(s => s.GameId == gameId && s.PlatformNumber == platformNumber).ToList();
        }

        public List<Rom> GetRomsByGameIds(List<int> gameIds)
        {
            return Query(s => gameIds.Contains(s.GameId)).ToList();
        }

        public List<Rom> GetRomsByScenePlatform(int gameId, int platformNumber)
        {
            return Query(s => s.GameId == gameId && s.ScenePlatformNumber == platformNumber).ToList();
        }

        public List<Rom> GetGameFileByFileId(int fileId)
        {
            return Query(e => e.RomFileId == fileId).ToList();
        }

        public List<Rom> GameFilesWithFiles(int gameId)
        {
            var builder = Builder()
                .Join<Rom, RomFile>((e, ef) => e.RomFileId == ef.Id)
                .Where<Rom>(e => e.GameId == gameId);

            return _database.QueryJoined<Rom, RomFile>(
                builder,
                (rom, romFile) =>
                {
                    rom.RomFile = romFile;
                    return rom;
                }).ToList();
        }

        public PagingSpec<Rom> GameFilesWithoutFiles(PagingSpec<Rom> pagingSpec, bool includeSpecials)
        {
            var currentTime = DateTime.UtcNow;
            var startingPlatformNumber = 1;

            if (includeSpecials)
            {
                startingPlatformNumber = 0;
            }

            pagingSpec.Records = GetPagedRecords(GameFilesWithoutFilesBuilder(currentTime, startingPlatformNumber), pagingSpec, PagedQuery);
            pagingSpec.TotalRecords = GetPagedRecordCount(GameFilesWithoutFilesBuilder(currentTime, startingPlatformNumber).SelectCountDistinct<Rom>(x => x.Id), pagingSpec);

            return pagingSpec;
        }

        public PagingSpec<Rom> GameFilesWhereCutoffUnmet(PagingSpec<Rom> pagingSpec, List<QualitiesBelowCutoff> qualitiesBelowCutoff, bool includeSpecials)
        {
            var startingPlatformNumber = 1;

            if (includeSpecials)
            {
                startingPlatformNumber = 0;
            }

            pagingSpec.Records = GetPagedRecords(GameFilesWhereCutoffUnmetBuilder(qualitiesBelowCutoff, startingPlatformNumber), pagingSpec, PagedQuery);

            var countTemplate = $"SELECT COUNT(*) FROM (SELECT /**select**/ FROM \"{TableMapping.Mapper.TableNameMapping(typeof(Rom))}\" /**join**/ /**innerjoin**/ /**leftjoin**/ /**where**/ /**groupby**/ /**having**/) AS \"Inner\"";
            pagingSpec.TotalRecords = GetPagedRecordCount(GameFilesWhereCutoffUnmetBuilder(qualitiesBelowCutoff, startingPlatformNumber).Select(typeof(Rom)), pagingSpec, countTemplate);

            return pagingSpec;
        }

        public List<Rom> FindGameFilesBySceneNumbering(int gameId, int platformNumber, int romNumber)
        {
            return Query(s => s.GameId == gameId && s.ScenePlatformNumber == platformNumber && s.SceneFileNumber == romNumber).ToList();
        }

        public List<Rom> FindGameFilesBySceneNumbering(int gameId, int sceneAbsoluteRomNumber)
        {
            return Query(s => s.GameId == gameId && s.SceneAbsoluteFileNumber == sceneAbsoluteRomNumber).ToList();
        }

        public List<Rom> GameFilesBetweenDates(DateTime startDate, DateTime endDate, bool includeUnmonitored, bool includeSpecials)
        {
            var builder = Builder().Where<Rom>(rg => rg.AirDateUtc >= startDate && rg.AirDateUtc <= endDate);

            if (!includeSpecials)
            {
                builder = builder.Where<Rom>(e => e.PlatformNumber != 0);
            }

            if (!includeUnmonitored)
            {
                builder = builder.Where<Rom>(e => e.Monitored == true)
                    .Join<Rom, Game>((l, r) => l.GameId == r.Id)
                    .Where<Game>(e => e.Monitored == true);
            }

            return Query(builder);
        }

        public void SetMonitoredFlat(Rom rom, bool monitored)
        {
            rom.Monitored = monitored;
            SetFields(rom, p => p.Monitored);

            ModelUpdated(rom, true);
        }

        public void SetMonitoredByPlatform(int gameId, int platformNumber, bool monitored)
        {
            using (var conn = _database.OpenConnection())
            {
                conn.Execute("UPDATE \"Roms\" SET \"Monitored\" = @monitored WHERE \"GameId\" = @gameId AND \"PlatformNumber\" = @platformNumber AND \"Monitored\" != @monitored",
                    new { gameId = gameId, platformNumber = platformNumber, monitored = monitored });
            }
        }

        public void SetMonitored(IEnumerable<int> ids, bool monitored)
        {
            var roms = ids.Select(x => new Rom { Id = x, Monitored = monitored }).ToList();
            SetFields(roms, p => p.Monitored);
        }

        public void SetFileId(Rom rom, int fileId)
        {
            rom.RomFileId = fileId;

            SetFields(rom, ep => ep.RomFileId);

            ModelUpdated(rom, true);
        }

        public void ClearFileId(Rom rom, bool unmonitor)
        {
            rom.RomFileId = 0;
            rom.Monitored &= !unmonitor;

            SetFields(rom, ep => ep.RomFileId, ep => ep.Monitored);

            ModelUpdated(rom, true);
        }

        private SqlBuilder GameFilesWithoutFilesBuilder(DateTime currentTime, int startingPlatformNumber) => Builder()
            .Join<Rom, Game>((l, r) => l.GameId == r.Id)
            .Where<Rom>(f => f.RomFileId == 0)
            .Where<Rom>(f => f.PlatformNumber >= startingPlatformNumber)
            .Where(BuildAirDateUtcCutoffWhereClause(currentTime));

        private string BuildAirDateUtcCutoffWhereClause(DateTime currentTime)
        {
            if (_database.DatabaseType == DatabaseType.PostgreSQL)
            {
                return string.Format("\"Roms\".\"AirDateUtc\" + make_interval(mins => \"Games\".\"Runtime\") <= '{0}'",
                                     currentTime.ToString("yyyy-MM-dd HH:mm:ss"));
            }

            return string.Format("datetime(strftime('%s', \"Roms\".\"AirDateUtc\") + \"Games\".\"Runtime\" * 60,  'unixepoch') <= '{0}'",
                                 currentTime.ToString("yyyy-MM-dd HH:mm:ss"));
        }

        private SqlBuilder GameFilesWhereCutoffUnmetBuilder(List<QualitiesBelowCutoff> qualitiesBelowCutoff, int startingPlatformNumber) => Builder()
            .Join<Rom, Game>((e, s) => e.GameId == s.Id)
            .LeftJoin<Rom, RomFile>((e, ef) => e.RomFileId == ef.Id)
            .Where<Rom>(e => e.RomFileId != 0)
            .Where<Rom>(e => e.PlatformNumber >= startingPlatformNumber)
            .Where(
                string.Format("({0})",
                    BuildQualityCutoffWhereClause(qualitiesBelowCutoff)))
            .GroupBy<Rom>(e => e.Id)
            .GroupBy<Game>(s => s.Id);

        private string BuildQualityCutoffWhereClause(List<QualitiesBelowCutoff> qualitiesBelowCutoff)
        {
            var clauses = new List<string>();

            foreach (var profile in qualitiesBelowCutoff)
            {
                foreach (var belowCutoff in profile.QualityIds)
                {
                    clauses.Add(string.Format("(\"Games\".\"QualityProfileId\" = {0} AND \"RomFiles\".\"Quality\" LIKE '%_quality_: {1},%')", profile.ProfileId, belowCutoff));
                }
            }

            return string.Format("({0})", string.Join(" OR ", clauses));
        }

        private Rom FindOneByAirDate(int gameId, string date)
        {
            var roms = Query(s => s.GameId == gameId && s.AirDate == date).ToList();

            if (!roms.Any())
            {
                return null;
            }

            if (roms.Count == 1)
            {
                return roms.First();
            }

            _logger.Debug("Multiple roms with the same air date were found, will exclude specials");

            var regularGameFiles = roms.Where(e => e.PlatformNumber > 0).ToList();

            if (regularGameFiles.Count == 1)
            {
                _logger.Debug("Left with one rom after excluding specials");
                return regularGameFiles.First();
            }

            throw new InvalidOperationException("Multiple roms with the same air date found");
        }
    }
}
