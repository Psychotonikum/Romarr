namespace Romarr.Core.Download.Clients.FreeboxDownload
{
    public class FreeboxDownloadException : DownloadClientException
    {
        public FreeboxDownloadException(string message)
            : base(message)
        {
        }
    }
}
