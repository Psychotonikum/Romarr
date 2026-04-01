using System.Linq;

namespace Romarr.Core.IndexerSearch.Definitions
{
    public class SpecialGameFileSearchCriteria : SearchCriteriaBase
    {
        public string[] GameFileQueryTitles { get; set; }

        public override string ToString()
        {
            var romTitles = GameFileQueryTitles.ToList();

            if (romTitles.Count > 0)
            {
                return $"[{Game.Title} ({Game.GameType})] Specials";
            }

            return $"[{Game.Title} ({Game.GameType}): {string.Join(",", GameFileQueryTitles)}]";
        }
    }
}
