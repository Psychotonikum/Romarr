using System.Collections.Generic;
using System.Threading.Tasks;
using Romarr.Core.Indexers;
using Romarr.Core.Parser.Model;
using Romarr.Core.ThingiProvider;

namespace Romarr.Core.Download
{
    public interface IDownloadClient : IProvider
    {
        DownloadProtocol Protocol { get; }
        Task<string> Download(RemoteRom remoteRom, IIndexer indexer);
        IEnumerable<DownloadClientItem> GetItems();
        DownloadClientItem GetImportItem(DownloadClientItem item, DownloadClientItem previousImportAttempt);
        void RemoveItem(DownloadClientItem item, bool deleteData);
        DownloadClientInfo GetStatus();
        void MarkItemAsImported(DownloadClientItem downloadClientItem);
    }
}
