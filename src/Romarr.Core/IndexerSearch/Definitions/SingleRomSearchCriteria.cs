using System.Linq;

namespace Romarr.Core.IndexerSearch.Definitions
{
    public class SingleGameFileSearchCriteria : SearchCriteriaBase
    {
        public int FileNumber { get; set; }
        public int PlatformNumber { get; set; }

        public override string ToString()
        {
            var platformName = Game.Platforms?.FirstOrDefault(p => p.PlatformNumber == PlatformNumber)?.Title ?? $"Platform {PlatformNumber}";
            var romTitle = Roms?.FirstOrDefault()?.Title ?? $"ROM {FileNumber}";

            return $"[{Game.Title} | {platformName} - {romTitle}]";
        }
    }
}
