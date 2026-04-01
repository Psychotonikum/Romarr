using System.Collections.Generic;
using System.Linq;
using Romarr.Core.Datastore;
using Romarr.Core.Messaging.Events;

namespace Romarr.Core.Download.History
{
    public interface IDownloadHistoryRepository : IBasicRepository<DownloadHistory>
    {
        List<DownloadHistory> FindByDownloadId(string downloadId);
        void DeleteByGameIds(List<int> gameIds);
    }

    public class DownloadHistoryRepository : BasicRepository<DownloadHistory>, IDownloadHistoryRepository
    {
        public DownloadHistoryRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }

        public List<DownloadHistory> FindByDownloadId(string downloadId)
        {
            return Query(h => h.DownloadId == downloadId).OrderByDescending(h => h.Date).ToList();
        }

        public void DeleteByGameIds(List<int> gameIds)
        {
            Delete(r => gameIds.Contains(r.GameId));
        }
    }
}
