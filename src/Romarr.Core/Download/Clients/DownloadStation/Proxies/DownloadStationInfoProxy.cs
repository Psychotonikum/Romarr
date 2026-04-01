using System.Collections.Generic;
using NLog;
using Romarr.Common.Cache;
using Romarr.Common.Http;

namespace Romarr.Core.Download.Clients.DownloadStation.Proxies
{
    public interface IDownloadStationInfoProxy : IDiskStationProxy
    {
        Dictionary<string, object> GetConfig(DownloadStationSettings settings);
    }

    public class DownloadStationInfoProxy : DiskStationProxyBase, IDownloadStationInfoProxy
    {
        public DownloadStationInfoProxy(IHttpClient httpClient, ICacheManager cacheManager, Logger logger)
            : base(DiskStationApi.DownloadStationInfo, "SYNO.DownloadStation.Info", httpClient, cacheManager, logger)
        {
        }

        public Dictionary<string, object> GetConfig(DownloadStationSettings settings)
        {
            var requestBuilder = BuildRequest(settings, "getConfig", 1);

            var response = ProcessRequest<Dictionary<string, object>>(requestBuilder, "get config", settings);

            return response.Data;
        }
    }
}
