using System;
using System.Collections.Generic;
using System.Linq;
using Romarr.Core.Datastore;
using Romarr.Core.Messaging.Events;
using Romarr.Core.Qualities;
using Romarr.Core.Games;

namespace Romarr.Core.History
{
    public interface IHistoryRepository : IBasicRepository<FileHistory>
    {
        FileHistory MostRecentForRom(int romId);
        List<FileHistory> FindByRomId(int romId);
        FileHistory MostRecentForDownloadId(string downloadId);
        List<FileHistory> FindByDownloadId(string downloadId);
        List<FileHistory> GetBySeries(int gameId, FileHistoryEventType? eventType);
        List<FileHistory> GetByPlatform(int gameId, int platformNumber, FileHistoryEventType? eventType);
        List<FileHistory> GetByGameFile(int romId, FileHistoryEventType? eventType);
        List<FileHistory> FindDownloadHistory(int idGameId, QualityModel quality);
        void DeleteForSeries(List<int> gameIds);
        List<FileHistory> Since(DateTime date, FileHistoryEventType? eventType);
        PagingSpec<FileHistory> GetPaged(PagingSpec<FileHistory> pagingSpec, int[] languages, int[] qualities);
    }

    public class HistoryRepository : BasicRepository<FileHistory>, IHistoryRepository
    {
        public HistoryRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }

        public FileHistory MostRecentForRom(int romId)
        {
            return Query(h => h.FileId == romId).MaxBy(h => h.Date);
        }

        public List<FileHistory> FindByRomId(int romId)
        {
            return Query(h => h.FileId == romId)
                        .OrderByDescending(h => h.Date)
                        .ToList();
        }

        public FileHistory MostRecentForDownloadId(string downloadId)
        {
            return Query(h => h.DownloadId == downloadId).MaxBy(h => h.Date);
        }

        public List<FileHistory> FindByDownloadId(string downloadId)
        {
            return Query(h => h.DownloadId == downloadId);
        }

        public List<FileHistory> GetBySeries(int gameId, FileHistoryEventType? eventType)
        {
            var builder = Builder().Join<FileHistory, Game>((h, a) => h.GameId == a.Id)
                                   .Join<FileHistory, Rom>((h, a) => h.FileId == a.Id)
                                   .Where<FileHistory>(h => h.GameId == gameId);

            if (eventType.HasValue)
            {
                builder.Where<FileHistory>(h => h.EventType == eventType);
            }

            return Query(builder).OrderByDescending(h => h.Date).ToList();
        }

        public List<FileHistory> GetByPlatform(int gameId, int platformNumber, FileHistoryEventType? eventType)
        {
            var builder = Builder()
                .Join<FileHistory, Rom>((h, a) => h.FileId == a.Id)
                .Join<FileHistory, Game>((h, a) => h.GameId == a.Id)
                .Where<FileHistory>(h => h.GameId == gameId && h.Rom.PlatformNumber == platformNumber);

            if (eventType.HasValue)
            {
                builder.Where<FileHistory>(h => h.EventType == eventType);
            }

            return _database.QueryJoined<FileHistory, Rom>(
                builder,
                (history, rom) =>
                {
                    history.Rom = rom;
                    return history;
                }).OrderByDescending(h => h.Date).ToList();
        }

        public List<FileHistory> GetByGameFile(int romId, FileHistoryEventType? eventType)
        {
            var builder = Builder()
                .Join<FileHistory, Game>((h, a) => h.GameId == a.Id)
                .Join<FileHistory, Rom>((h, a) => h.FileId == a.Id)
                .Where<FileHistory>(h => h.FileId == romId);

            if (eventType.HasValue)
            {
                builder.Where<FileHistory>(h => h.EventType == eventType);
            }

            return Query(builder).OrderByDescending(h => h.Date).ToList();
        }

        public List<FileHistory> FindDownloadHistory(int idGameId, QualityModel quality)
        {
            return Query(h =>
                 h.GameId == idGameId &&
                 h.Quality == quality &&
                 (h.EventType == FileHistoryEventType.Grabbed ||
                 h.EventType == FileHistoryEventType.DownloadFailed ||
                 h.EventType == FileHistoryEventType.DownloadFolderImported))
                 .ToList();
        }

        public void DeleteForSeries(List<int> gameIds)
        {
            Delete(c => gameIds.Contains(c.GameId));
        }

        public List<FileHistory> Since(DateTime date, FileHistoryEventType? eventType)
        {
            var builder = Builder()
                .Join<FileHistory, Game>((h, a) => h.GameId == a.Id)
                .Join<FileHistory, Rom>((h, a) => h.FileId == a.Id)
                .Where<FileHistory>(x => x.Date >= date);

            if (eventType.HasValue)
            {
                builder.Where<FileHistory>(h => h.EventType == eventType);
            }

            return _database.QueryJoined<FileHistory, Game, Rom>(builder, (history, game, rom) =>
            {
                history.Game = game;
                history.Rom = rom;
                return history;
            }).OrderBy(h => h.Date).ToList();
        }

        public PagingSpec<FileHistory> GetPaged(PagingSpec<FileHistory> pagingSpec, int[] languages, int[] qualities)
        {
            pagingSpec.Records = GetPagedRecords(PagedBuilder(languages, qualities), pagingSpec, PagedQuery);

            var countTemplate = $"SELECT COUNT(*) FROM (SELECT /**select**/ FROM \"{TableMapping.Mapper.TableNameMapping(typeof(FileHistory))}\" /**join**/ /**innerjoin**/ /**leftjoin**/ /**where**/ /**groupby**/ /**having**/) AS \"Inner\"";
            pagingSpec.TotalRecords = GetPagedRecordCount(PagedBuilder(languages, qualities).Select(typeof(FileHistory)), pagingSpec, countTemplate);

            return pagingSpec;
        }

        private SqlBuilder PagedBuilder(int[] languages, int[] qualities)
        {
            var builder = Builder()
                .Join<FileHistory, Game>((h, a) => h.GameId == a.Id)
                .Join<FileHistory, Rom>((h, a) => h.FileId == a.Id);

            if (languages is { Length: > 0 })
            {
                builder.Where($"({BuildLanguageWhereClause(languages)})");
            }

            if (qualities is { Length: > 0 })
            {
                builder.Where($"({BuildQualityWhereClause(qualities)})");
            }

            return builder;
        }

        protected override IEnumerable<FileHistory> PagedQuery(SqlBuilder builder) =>
            _database.QueryJoined<FileHistory, Game, Rom>(builder, (history, game, rom) =>
            {
                history.Game = game;
                history.Rom = rom;
                return history;
            });

        private string BuildLanguageWhereClause(int[] languages)
        {
            var clauses = new List<string>();

            foreach (var language in languages)
            {
                // There are 4 different types of values we should see:
                // - Not the last value in the array
                // - When it's the last value in the array and on different OSes
                // - When it was converted from a single language

                clauses.Add($"\"{TableMapping.Mapper.TableNameMapping(typeof(FileHistory))}\".\"Languages\" LIKE '[% {language},%]'");
                clauses.Add($"\"{TableMapping.Mapper.TableNameMapping(typeof(FileHistory))}\".\"Languages\" LIKE '[% {language}' || CHAR(13) || '%]'");
                clauses.Add($"\"{TableMapping.Mapper.TableNameMapping(typeof(FileHistory))}\".\"Languages\" LIKE '[% {language}' || CHAR(10) || '%]'");
                clauses.Add($"\"{TableMapping.Mapper.TableNameMapping(typeof(FileHistory))}\".\"Languages\" LIKE '[{language}]'");
            }

            return $"({string.Join(" OR ", clauses)})";
        }

        private string BuildQualityWhereClause(int[] qualities)
        {
            var clauses = new List<string>();

            foreach (var quality in qualities)
            {
                clauses.Add($"\"{TableMapping.Mapper.TableNameMapping(typeof(FileHistory))}\".\"Quality\" LIKE '%_quality_: {quality},%'");
            }

            return $"({string.Join(" OR ", clauses)})";
        }
    }
}
