using System.Collections.Generic;
using Romarr.Core.DataAugmentation.Scene;
using Romarr.Core.Games;

namespace Romarr.Core.IndexerSearch.Definitions
{
    public class SceneGameFileMapping
    {
        public Rom Rom { get; set; }
        public SearchMode SearchMode { get; set; }
        public List<string> SceneTitles { get; set; }
        public int PlatformNumber { get; set; }
        public int FileNumber { get; set; }
        public int? AbsoluteFileNumber { get; set; }

        public override int GetHashCode()
        {
            return SearchMode.GetHashCode() ^ PlatformNumber.GetHashCode() ^ FileNumber.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var other = obj as SceneGameFileMapping;

            if (object.ReferenceEquals(other, null))
            {
                return false;
            }

            return PlatformNumber == other.PlatformNumber && FileNumber == other.FileNumber && SearchMode == other.SearchMode;
        }
    }
}
