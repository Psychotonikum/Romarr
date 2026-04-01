using Romarr.Core.Parser.Model;
using Romarr.Core.Qualities;
using Romarr.Core.Games;

namespace Romarr.Core.Notifications
{
    public class GrabMessage
    {
        public string Message { get; set; }
        public Game Game { get; set; }
        public RemoteRom Rom { get; set; }
        public QualityModel Quality { get; set; }
        public string DownloadClientType { get; set; }
        public string DownloadClientName { get; set; }
        public string DownloadId { get; set; }

        public override string ToString()
        {
            return Message;
        }
    }
}
