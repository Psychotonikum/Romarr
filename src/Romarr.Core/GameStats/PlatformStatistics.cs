using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Romarr.Common.Extensions;
using Romarr.Core.Datastore;

namespace Romarr.Core.GameStats
{
    public class PlatformStatistics : ResultSet
    {
        public int GameId { get; set; }
        public int PlatformNumber { get; set; }
        public string NextAiringString { get; set; }
        public string PreviousAiringString { get; set; }
        public string LastAiredString { get; set; }
        public int RomFileCount { get; set; }
        public int GameFileCount { get; set; }
        public int AvailableGameFileCount { get; set; }
        public int TotalGameFileCount { get; set; }
        public int MonitoredGameFileCount { get; set; }
        public long SizeOnDisk { get; set; }
        public string ReleaseGroupsString { get; set; }

        public DateTime? NextAiring
        {
            get
            {
                DateTime nextAiring;

                try
                {
                    if (!DateTime.TryParse(NextAiringString, DateTimeFormatInfo.InvariantInfo, DateTimeStyles.AssumeUniversal, out nextAiring))
                    {
                        return null;
                    }
                }
                catch (ArgumentOutOfRangeException)
                {
                    // GHI 3518: Can throw on mono (6.x?) despite being a Try*
                    return null;
                }

                return nextAiring;
            }
        }

        public DateTime? PreviousAiring
        {
            get
            {
                DateTime previousAiring;

                try
                {
                    if (!DateTime.TryParse(PreviousAiringString, DateTimeFormatInfo.InvariantInfo, DateTimeStyles.AssumeUniversal, out previousAiring))
                    {
                        return null;
                    }
                }
                catch (ArgumentOutOfRangeException)
                {
                    // GHI 3518: Can throw on mono (6.x?) despite being a Try*
                    return null;
                }

                return previousAiring;
            }
        }

        public DateTime? LastAired
        {
            get
            {
                DateTime lastAired;

                try
                {
                    if (!DateTime.TryParse(LastAiredString, DateTimeFormatInfo.InvariantInfo, DateTimeStyles.AssumeUniversal, out lastAired))
                    {
                        return null;
                    }
                }
                catch (ArgumentOutOfRangeException)
                {
                    // GHI 3518: Can throw on mono (6.x?) despite being a Try*
                    return null;
                }

                return lastAired;
            }
        }

        public List<string> ReleaseGroups
        {
            get
            {
                var releasegroups = new List<string>();

                if (ReleaseGroupsString.IsNotNullOrWhiteSpace())
                {
                    releasegroups = ReleaseGroupsString
                        .Split('|')
                        .Distinct()
                        .Where(rg => rg.IsNotNullOrWhiteSpace())
                        .OrderBy(rg => rg)
                        .ToList();
                }

                return releasegroups;
            }
        }
    }
}
