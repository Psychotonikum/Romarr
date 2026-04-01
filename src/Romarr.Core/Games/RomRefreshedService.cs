using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using Romarr.Common.Cache;
using Romarr.Common.Extensions;
using Romarr.Core.IndexerSearch;
using Romarr.Core.Messaging.Commands;
using Romarr.Core.Messaging.Events;
using Romarr.Core.Games.Events;

namespace Romarr.Core.Games
{
    public interface IGameFileRefreshedService
    {
        void Search(int gameId);
    }

    public class GameFileRefreshedService : IGameFileRefreshedService, IHandle<RomInfoRefreshedEvent>
    {
        private readonly IManageCommandQueue _commandQueueManager;
        private readonly IRomService _romService;
        private readonly Logger _logger;
        private readonly ICached<List<int>> _searchCache;

        public GameFileRefreshedService(ICacheManager cacheManager,
                                   IManageCommandQueue commandQueueManager,
                                   IRomService gameFileService,
                                   Logger logger)
        {
            _commandQueueManager = commandQueueManager;
            _romService = gameFileService;
            _logger = logger;
            _searchCache = cacheManager.GetCache<List<int>>(GetType());
        }

        public void Search(int gameId)
        {
            var previouslyAired = _searchCache.Find(gameId.ToString());

            if (previouslyAired != null && previouslyAired.Any())
            {
                var missing = previouslyAired.Select(e => _romService.GetGameFile(e)).Where(e => !e.HasFile).ToList();

                if (missing.Any())
                {
                    _commandQueueManager.Push(new RomSearchCommand(missing.Select(e => e.Id).ToList()));
                }
            }

            _searchCache.Remove(gameId.ToString());
        }

        public void Handle(RomInfoRefreshedEvent message)
        {
            if (message.Game.AddOptions == null)
            {
                var toSearch = new List<int>();

                if (!message.Game.Monitored)
                {
                    _logger.Debug("Game is not monitored");
                    return;
                }

                var previouslyAired = message.Added.Where(a =>
                        a.AirDateUtc.HasValue &&
                        a.AirDateUtc.Value.Between(DateTime.UtcNow.AddDays(-14), DateTime.UtcNow.AddDays(1)) &&
                        a.Monitored)
                    .Select(e => e.Id)
                    .ToList();

                if (previouslyAired.Empty())
                {
                    _logger.Debug("Newly added roms all air in the future");
                }

                toSearch.AddRange(previouslyAired);

                var absoluteRomNumberAdded = message.Updated.Where(a =>
                        a.AbsoluteRomNumberAdded &&
                        a.AirDateUtc.HasValue &&
                        a.AirDateUtc.Value.Between(DateTime.UtcNow.AddDays(-14), DateTime.UtcNow.AddDays(1)) &&
                        a.Monitored)
                    .Select(e => e.Id)
                    .ToList();

                if (absoluteRomNumberAdded.Empty())
                {
                    _logger.Debug("No updated roms recently aired and had absolute rom number added");
                }

                toSearch.AddRange(absoluteRomNumberAdded);

                if (toSearch.Any())
                {
                    _searchCache.Set(message.Game.Id.ToString(), toSearch.Distinct().ToList());
                }
            }
        }
    }
}
