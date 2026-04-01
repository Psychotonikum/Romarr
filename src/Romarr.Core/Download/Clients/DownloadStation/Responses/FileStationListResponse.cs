using System.Collections.Generic;

namespace Romarr.Core.Download.Clients.DownloadStation.Responses
{
    public class FileStationListResponse
    {
        public List<FileStationListFileInfoResponse> Files { get; set; }
    }
}
