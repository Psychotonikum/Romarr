using System;
using Romarr.Core.Datastore;

namespace Romarr.Core.Extras.Files
{
    public abstract class ExtraFile : ModelBase
    {
        public int GameId { get; set; }
        public int? RomFileId { get; set; }
        public int? PlatformNumber { get; set; }
        public string RelativePath { get; set; }
        public DateTime Added { get; set; }
        public DateTime LastUpdated { get; set; }
        public string Extension { get; set; }

        public override string ToString()
        {
            return $"[{Id}] {RelativePath}";
        }
    }
}
