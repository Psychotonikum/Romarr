using System.Linq;
using Romarr.Common.Extensions;
using Romarr.Core.Download;
using Romarr.Core.History;
using Romarr.Core.Parser.Model;

namespace Romarr.Core.MediaFiles.GameFileImport.Aggregation.Aggregators
{
    public class AggregateReleaseInfo : IAggregateLocalGameFile
    {
        public int Order => 1;

        private readonly IHistoryService _historyService;

        public AggregateReleaseInfo(IHistoryService historyService)
        {
            _historyService = historyService;
        }

        public LocalGameFile Aggregate(LocalGameFile localRom, DownloadClientItem downloadClientItem)
        {
            if (downloadClientItem == null)
            {
                return localRom;
            }

            var grabbedHistories = _historyService.FindByDownloadId(downloadClientItem.DownloadId)
                .Where(h => h.EventType == FileHistoryEventType.Grabbed)
                .ToList();

            if (grabbedHistories.Empty())
            {
                return localRom;
            }

            localRom.Release = new GrabbedReleaseInfo(grabbedHistories);

            return localRom;
        }
    }
}
