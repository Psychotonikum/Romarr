using System.Collections.Generic;
using Romarr.Core.Datastore;

namespace Romarr.Core.Games
{
    public class Platform : IEmbeddedDocument
    {
        public Platform()
        {
            Images = new List<MediaCover.MediaCover>();
        }

        public int PlatformNumber { get; set; }
        public string Title { get; set; }
        public bool Monitored { get; set; }
        public List<MediaCover.MediaCover> Images { get; set; }
    }
}
