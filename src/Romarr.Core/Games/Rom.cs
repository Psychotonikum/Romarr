using System;
using System.Collections.Generic;
using Romarr.Common.Extensions;
using Romarr.Core.Datastore;
using Romarr.Core.MediaFiles;

namespace Romarr.Core.Games
{
    public class Rom : ModelBase, IComparable
    {
        public Rom()
        {
            Images = new List<MediaCover.MediaCover>();
        }

        public const string AIR_DATE_FORMAT = "yyyy-MM-dd";

        public int GameId { get; set; }
        public int IgdbId { get; set; }
        public int RomFileId { get; set; }
        public int PlatformNumber { get; set; }
        public int FileNumber { get; set; }
        public string Title { get; set; }
        public string AirDate { get; set; }
        public DateTime? AirDateUtc { get; set; }
        public string Overview { get; set; }
        public bool Monitored { get; set; }
        public int? AbsoluteFileNumber { get; set; }
        public int? SceneAbsoluteFileNumber { get; set; }
        public int? ScenePlatformNumber { get; set; }
        public int? SceneFileNumber { get; set; }
        public int? AiredAfterPlatformNumber { get; set; }
        public int? AiredBeforePlatformNumber { get; set; }
        public int? AiredBeforeRomNumber { get; set; }
        public bool UnverifiedSceneNumbering { get; set; }
        public Ratings Ratings { get; set; }
        public List<MediaCover.MediaCover> Images { get; set; }
        public DateTime? LastSearchTime { get; set; }
        public int Runtime { get; set; }
        public string FinaleType { get; set; }
        public RomFileType RomType { get; set; }

        public string GameTitle { get; private set; }

        public LazyLoaded<RomFile> RomFile { get; set; }

        public Game Game { get; set; }

        public bool HasFile => RomFileId > 0;
        public bool AbsoluteRomNumberAdded { get; set; }

        public override string ToString()
        {
            return string.Format("[{0}]{1}", Id, Title.NullSafe());
        }

        public int CompareTo(object obj)
        {
            var other = (Rom)obj;

            if (PlatformNumber > other.PlatformNumber)
            {
                return 1;
            }

            if (PlatformNumber < other.PlatformNumber)
            {
                return -1;
            }

            if (FileNumber > other.FileNumber)
            {
                return 1;
            }

            if (FileNumber < other.FileNumber)
            {
                return -1;
            }

            return 0;
        }
    }
}
