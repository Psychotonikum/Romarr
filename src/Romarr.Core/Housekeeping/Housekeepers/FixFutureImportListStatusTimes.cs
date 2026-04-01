using Romarr.Core.ImportLists;

namespace Romarr.Core.Housekeeping.Housekeepers
{
    public class FixFutureImportListStatusTimes : FixFutureProviderStatusTimes<ImportListStatus>, IHousekeepingTask
    {
        public FixFutureImportListStatusTimes(IImportListStatusRepository importListStatusRepository)
            : base(importListStatusRepository)
        {
        }
    }
}
