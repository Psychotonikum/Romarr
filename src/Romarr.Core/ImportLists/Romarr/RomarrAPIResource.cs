using System.Collections.Generic;

namespace Romarr.Core.ImportLists.Romarr
{
    public class RomarrSeries
    {
        public string Title { get; set; }
        public string SortTitle { get; set; }
        public int IgdbId { get; set; }
        public string Overview { get; set; }
        public List<MediaCover.MediaCover> Images { get; set; }
        public bool Monitored { get; set; }
        public int Year { get; set; }
        public string TitleSlug { get; set; }
        public int QualityProfileId { get; set; }
        public int LanguageProfileId { get; set; }
        public string RootFolderPath { get; set; }
        public List<RomarrPlatform> Platforms { get; set; }
        public HashSet<int> Tags { get; set; }
    }

    public class RomarrProfile
    {
        public string Name { get; set; }
        public int Id { get; set; }
    }

    public class RomarrTag
    {
        public string Label { get; set; }
        public int Id { get; set; }
    }

    public class RomarrRootFolder
    {
        public string Path { get; set; }
        public int Id { get; set; }
    }

    public class RomarrPlatform
    {
        public int PlatformNumber { get; set; }
        public bool Monitored { get; set; }
    }
}
