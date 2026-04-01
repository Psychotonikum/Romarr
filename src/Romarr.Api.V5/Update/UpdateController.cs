using Microsoft.AspNetCore.Mvc;
using NLog;
using Romarr.Common.EnvironmentInfo;
using Romarr.Core.Configuration;
using Romarr.Core.Update;
using Romarr.Core.Update.History;
using Romarr.Http;

namespace Romarr.Api.V5.Update
{
    [V5ApiController]
    public class UpdateController : Controller
    {
        private readonly IRecentUpdateProvider _recentUpdateProvider;
        private readonly IUpdateHistoryService _updateHistoryService;
        private readonly IConfigFileProvider _configFileProvider;
        private readonly Logger _logger;

        public UpdateController(IRecentUpdateProvider recentUpdateProvider, IUpdateHistoryService updateHistoryService, IConfigFileProvider configFileProvider, Logger logger)
        {
            _recentUpdateProvider = recentUpdateProvider;
            _updateHistoryService = updateHistoryService;
            _configFileProvider = configFileProvider;
            _logger = logger;
        }

        [HttpGet]
        [Produces("application/json")]
        public List<UpdateResource> GetRecentUpdates()
        {
            List<UpdateResource> resources;

            try
            {
                resources = _recentUpdateProvider.GetRecentUpdatePackages()
                                                 .OrderByDescending(u => u.Version)
                                                 .ToResource();
            }
            catch (Exception ex)
            {
                _logger.Warn(ex, "Unable to fetch recent updates");
                return new List<UpdateResource>();
            }

            if (resources.Any())
            {
                var first = resources.First();
                first.Latest = true;

                if (first.Version > BuildInfo.Version)
                {
                    first.Installable = true;
                }

                var installed = resources.SingleOrDefault(r => r.Version == BuildInfo.Version);

                if (installed != null)
                {
                    installed.Installed = true;
                }

                if (!_configFileProvider.LogDbEnabled)
                {
                    return resources;
                }

                var updateHistory = _updateHistoryService.InstalledSince(resources.Last().ReleaseDate);
                var installDates = updateHistory
                                                        .DistinctBy(v => v.Version)
                                                        .ToDictionary(v => v.Version);

                foreach (var resource in resources)
                {
                    if (installDates.TryGetValue(resource.Version, out var installDate))
                    {
                        resource.InstalledOn = installDate.Date;
                    }
                }
            }

            return resources;
        }
    }
}
