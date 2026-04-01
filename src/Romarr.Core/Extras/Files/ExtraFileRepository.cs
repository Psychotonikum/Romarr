using System.Collections.Generic;
using System.Linq;
using Romarr.Core.Datastore;
using Romarr.Core.Messaging.Events;

namespace Romarr.Core.Extras.Files
{
    public interface IExtraFileRepository<TExtraFile> : IBasicRepository<TExtraFile>
        where TExtraFile : ExtraFile, new()
    {
        void DeleteForGameIds(List<int> gameIds);
        void DeleteForPlatform(int gameId, int platformNumber);
        void DeleteForRomFile(int romFileId);
        List<TExtraFile> GetFilesBySeries(int gameId);
        List<TExtraFile> GetFilesByPlatform(int gameId, int platformNumber);
        List<TExtraFile> GetFilesByRomFile(int romFileId);
        TExtraFile FindByPath(int gameId, string path);
    }

    public class ExtraFileRepository<TExtraFile> : BasicRepository<TExtraFile>, IExtraFileRepository<TExtraFile>
        where TExtraFile : ExtraFile, new()
    {
        public ExtraFileRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }

        public void DeleteForGameIds(List<int> gameIds)
        {
            Delete(c => gameIds.Contains(c.GameId));
        }

        public void DeleteForPlatform(int gameId, int platformNumber)
        {
            Delete(c => c.GameId == gameId && c.PlatformNumber == platformNumber);
        }

        public void DeleteForRomFile(int romFileId)
        {
            Delete(c => c.RomFileId == romFileId);
        }

        public List<TExtraFile> GetFilesBySeries(int gameId)
        {
            return Query(c => c.GameId == gameId);
        }

        public List<TExtraFile> GetFilesByPlatform(int gameId, int platformNumber)
        {
            return Query(c => c.GameId == gameId && c.PlatformNumber == platformNumber);
        }

        public List<TExtraFile> GetFilesByRomFile(int romFileId)
        {
            return Query(c => c.RomFileId == romFileId);
        }

        public TExtraFile FindByPath(int gameId, string path)
        {
            return Query(c => c.GameId == gameId && c.RelativePath == path).SingleOrDefault();
        }
    }
}
