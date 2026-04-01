using System.Linq;
using Romarr.Core.Parser;
using Romarr.Core.Games;

namespace Romarr.Core.Housekeeping.Housekeepers
{
    public class UpdateCleanTitleForSeries : IHousekeepingTask
    {
        private readonly IGameRepository _gameRepository;

        public UpdateCleanTitleForSeries(IGameRepository seriesRepository)
        {
            _gameRepository = seriesRepository;
        }

        public void Clean()
        {
            var game = _gameRepository.All().ToList();

            game.ForEach(s =>
            {
                var cleanTitle = s.Title.CleanGameTitle();
                if (s.CleanTitle != cleanTitle)
                {
                    s.CleanTitle = cleanTitle;
                    _gameRepository.Update(s);
                }
            });
        }
    }
}
