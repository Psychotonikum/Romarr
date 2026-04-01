using System;

namespace Romarr.Core.MetadataSource.SkyHook.Resource
{
    public class RomResource
    {
        public int IgdbId { get; set; }
        public int PlatformNumber { get; set; }
        public int FileNumber { get; set; }
        public int? AbsoluteFileNumber { get; set; }
        public int? AiredAfterPlatformNumber { get; set; }
        public int? AiredBeforePlatformNumber { get; set; }
        public int? AiredBeforeRomNumber { get; set; }
        public string Title { get; set; }
        public string AirDate { get; set; }
        public DateTime? AirDateUtc { get; set; }
        public int Runtime { get; set; }
        public string FinaleType { get; set; }
        public RatingResource Rating { get; set; }
        public string Overview { get; set; }
        public string Image { get; set; }
    }
}
