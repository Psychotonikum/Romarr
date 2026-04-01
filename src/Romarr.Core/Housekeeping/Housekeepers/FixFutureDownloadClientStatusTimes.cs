using Romarr.Core.Download;

namespace Romarr.Core.Housekeeping.Housekeepers
{
    public class FixFutureDownloadClientStatusTimes : FixFutureProviderStatusTimes<DownloadClientStatus>, IHousekeepingTask
    {
        public FixFutureDownloadClientStatusTimes(IDownloadClientStatusRepository downloadClientStatusRepository)
            : base(downloadClientStatusRepository)
        {
        }
    }
}
