using NLog;
using Romarr.Core.IndexerSearch;
using Romarr.Core.MediaFiles.Events;
using Romarr.Core.Messaging.Commands;
using Romarr.Core.Messaging.Events;
using Romarr.Core.Games.Events;

namespace Romarr.Core.Games
{
    public class SeriesScannedHandler : IHandle<SeriesScannedEvent>,
                                        IHandle<SeriesScanSkippedEvent>
    {
        private readonly IFileMonitoredService _romMonitoredService;
        private readonly IGameService _gameService;
        private readonly IManageCommandQueue _commandQueueManager;
        private readonly IGameFileRefreshedService _romRefreshedService;
        private readonly IEventAggregator _eventAggregator;

        private readonly Logger _logger;

        public SeriesScannedHandler(IFileMonitoredService gameFileMonitoredService,
                                    IGameService seriesService,
                                    IManageCommandQueue commandQueueManager,
                                    IGameFileRefreshedService gameFileRefreshedService,
                                    IEventAggregator eventAggregator,
                                    Logger logger)
        {
            _romMonitoredService = gameFileMonitoredService;
            _gameService = seriesService;
            _commandQueueManager = commandQueueManager;
            _romRefreshedService = gameFileRefreshedService;
            _eventAggregator = eventAggregator;
            _logger = logger;
        }

        private void HandleScanEvents(Game game)
        {
            var addOptions = game.AddOptions;

            if (addOptions == null)
            {
                _romRefreshedService.Search(game.Id);
                return;
            }

            _logger.Info("[{0}] was recently added, performing post-add actions", game.Title);
            _romMonitoredService.SetGameFileMonitoredStatus(game, addOptions);

            _eventAggregator.PublishEvent(new SeriesAddCompletedEvent(game));

            // If both options are enabled search for the whole game, which will only include monitored roms.
            // This way multiple searches for the same platform are skipped, though a platform that can't be upgraded may be
            // searched, but the logs will be more explicit.

            if (addOptions.SearchForMissingGameFiles && addOptions.SearchForCutoffUnmetGameFiles)
            {
                _commandQueueManager.Push(new GameSearchCommand(game.Id));
            }
            else
            {
                if (addOptions.SearchForMissingGameFiles)
                {
                    _commandQueueManager.Push(new MissingRomSearchCommand(game.Id));
                }

                if (addOptions.SearchForCutoffUnmetGameFiles)
                {
                    _commandQueueManager.Push(new CutoffUnmetRomSearchCommand(game.Id));
                }
            }

            game.AddOptions = null;
            _gameService.RemoveAddOptions(game);
        }

        public void Handle(SeriesScannedEvent message)
        {
            HandleScanEvents(message.Game);
        }

        public void Handle(SeriesScanSkippedEvent message)
        {
            HandleScanEvents(message.Game);
        }
    }
}
