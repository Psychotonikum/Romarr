using System.Collections.Generic;
using System.Linq;
using Romarr.Common.Extensions;
using Romarr.Core.Localization;
using Romarr.Core.Games;
using Romarr.Core.Games.Events;

namespace Romarr.Core.HealthCheck.Checks
{
    [CheckOn(typeof(GameUpdatedEvent))]
    [CheckOn(typeof(GameDeletedEvent))]
    [CheckOn(typeof(GameRefreshCompleteEvent))]
    public class RemovedSeriesCheck : HealthCheckBase, ICheckOnCondition<GameUpdatedEvent>, ICheckOnCondition<GameDeletedEvent>
    {
        private readonly IGameService _gameService;

        public RemovedSeriesCheck(IGameService seriesService, ILocalizationService localizationService)
            : base(localizationService)
        {
            _gameService = seriesService;
        }

        public override HealthCheck Check()
        {
            var deletedSeries = _gameService.GetAllGames().Where(v => v.Status == GameStatusType.Deleted && v.IgdbId > 0).ToList();

            if (deletedSeries.Empty())
            {
                return new HealthCheck(GetType());
            }

            var seriesText = deletedSeries.Select(s => $"{s.Title} (igdbid {s.IgdbId})").Join(", ");

            if (deletedSeries.Count == 1)
            {
                return new HealthCheck(GetType(),
                    HealthCheckResult.Error,
                    HealthCheckReason.RemovedSeriesSingle,
                    _localizationService.GetLocalizedString("RemovedSeriesSingleRemovedHealthCheckMessage", new Dictionary<string, object>
                    {
                        { "game", seriesText }
                    }),
                    "#game-removed-from-theigdb");
            }

            return new HealthCheck(GetType(),
                HealthCheckResult.Error,
                HealthCheckReason.RemovedSeriesMultiple,
                _localizationService.GetLocalizedString("RemovedSeriesMultipleRemovedHealthCheckMessage", new Dictionary<string, object>
                {
                    { "game", seriesText }
                }),
                "#game-removed-from-theigdb");
        }

        public bool ShouldCheckOnEvent(GameDeletedEvent deletedEvent)
        {
            return deletedEvent.Game.Any(s => s.Status == GameStatusType.Deleted);
        }

        public bool ShouldCheckOnEvent(GameUpdatedEvent updatedEvent)
        {
            return updatedEvent.Game.Status == GameStatusType.Deleted;
        }
    }
}
