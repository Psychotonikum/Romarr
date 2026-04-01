using System.Collections.Generic;
using Romarr.Core.DataAugmentation.Scene;
using Romarr.Core.Games;

namespace Romarr.Core.IndexerSearch.Definitions
{
    public class ScenePlatformMapping
    {
        public List<Rom> Roms { get; set; }
        public SceneGameFileMapping GameFileMapping { get; set; }
        public SearchMode SearchMode { get; set; }
        public List<string> SceneTitles { get; set; }
        public int PlatformNumber { get; set; }

        public override int GetHashCode()
        {
            return SearchMode.GetHashCode() ^ PlatformNumber.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var other = obj as ScenePlatformMapping;

            if (object.ReferenceEquals(other, null))
            {
                return false;
            }

            return PlatformNumber == other.PlatformNumber && SearchMode == other.SearchMode;
        }
    }
}
