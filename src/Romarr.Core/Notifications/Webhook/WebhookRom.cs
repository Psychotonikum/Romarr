using System;
using Romarr.Core.Games;

namespace Romarr.Core.Notifications.Webhook
{
    public class WebhookGameFile
    {
        public WebhookGameFile()
        {
        }

        public WebhookGameFile(Rom rom)
        {
            Id = rom.Id;
            PlatformNumber = rom.PlatformNumber;
            FileNumber = rom.FileNumber;
            Title = rom.Title;
            Overview = rom.Overview;
            AirDate = rom.AirDate;
            AirDateUtc = rom.AirDateUtc;
            GameId = rom.GameId;
            IgdbId = rom.IgdbId;
            FinaleType = rom.FinaleType;
        }

        public int Id { get; set; }
        public int FileNumber { get; set; }
        public int PlatformNumber { get; set; }
        public string Title { get; set; }
        public string Overview { get; set; }
        public string AirDate { get; set; }
        public DateTime? AirDateUtc { get; set; }
        public int GameId { get; set; }
        public int IgdbId { get; set; }
        public string FinaleType { get; set; }
    }
}
