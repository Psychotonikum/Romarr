using Romarr.Core.Configuration;
using Romarr.Core.Instrumentation;

namespace Romarr.Core.Housekeeping.Housekeepers
{
    public class TrimLogDatabase : IHousekeepingTask
    {
        private readonly ILogRepository _logRepo;
        private readonly IConfigFileProvider _configFileProvider;

        public TrimLogDatabase(ILogRepository logRepo, IConfigFileProvider configFileProvider)
        {
            _logRepo = logRepo;
            _configFileProvider = configFileProvider;
        }

        public void Clean()
        {
            if (!_configFileProvider.LogDbEnabled)
            {
                return;
            }

            _logRepo.Trim();
        }
    }
}
