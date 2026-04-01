using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Romarr.Common.Extensions;
using Romarr.Core.DataAugmentation.Scene;
using Romarr.Core.Datastore;
using Romarr.Core.Datastore.Events;
using Romarr.Core.MediaCover;
using Romarr.Core.MediaFiles;
using Romarr.Core.MediaFiles.Events;
using Romarr.Core.Messaging.Commands;
using Romarr.Core.Messaging.Events;
using Romarr.Core.RootFolders;
using Romarr.Core.GameStats;
using Romarr.Core.Games;
using Romarr.Core.Games.Commands;
using Romarr.Core.Games.Events;
using Romarr.Core.Validation;
using Romarr.Core.Validation.Paths;
using Romarr.SignalR;
using Romarr.Http;
using Romarr.Http.Extensions;
using Romarr.Http.REST;
using Romarr.Http.REST.Attributes;

namespace Romarr.Api.V3.Game
{
    [V3ApiController]
    public class GameController : RestControllerWithSignalR<GameResource, Romarr.Core.Games.Game>,
                                IHandle<FileImportedEvent>,
                                IHandle<RomFileDeletedEvent>,
                                IHandle<GameUpdatedEvent>,
                                IHandle<GameEditedEvent>,
                                IHandle<GameDeletedEvent>,
                                IHandle<SeriesRenamedEvent>,
                                IHandle<SeriesBulkEditedEvent>,
                                IHandle<MediaCoversUpdatedEvent>
    {
        private readonly IGameService _gameService;
        private readonly IAddGameService _addGameService;
        private readonly IGameStatisticsService _gameStatisticsService;
        private readonly ISceneMappingService _sceneMappingService;
        private readonly IMapCoversToLocal _coverMapper;
        private readonly IManageCommandQueue _commandQueueManager;
        private readonly IRootFolderService _rootFolderService;

        public GameController(IBroadcastSignalRMessage signalRBroadcaster,
                            IGameService seriesService,
                            IAddGameService addGameService,
                            IGameStatisticsService gameStatisticsService,
                            ISceneMappingService sceneMappingService,
                            IMapCoversToLocal coverMapper,
                            IManageCommandQueue commandQueueManager,
                            IRootFolderService rootFolderService,
                            RootFolderValidator rootFolderValidator,
                            MappedNetworkDriveValidator mappedNetworkDriveValidator,
                            GamePathValidator seriesPathValidator,
                            GameExistsValidator seriesExistsValidator,
                            GameAncestorValidator seriesAncestorValidator,
                            SystemFolderValidator systemFolderValidator,
                            QualityProfileExistsValidator qualityProfileExistsValidator,
                            RootFolderExistsValidator rootFolderExistsValidator,
                            GameFolderAsRootFolderValidator seriesFolderAsRootFolderValidator)
            : base(signalRBroadcaster)
        {
            _gameService = seriesService;
            _addGameService = addGameService;
            _gameStatisticsService = gameStatisticsService;
            _sceneMappingService = sceneMappingService;

            _coverMapper = coverMapper;
            _commandQueueManager = commandQueueManager;
            _rootFolderService = rootFolderService;

            SharedValidator.RuleFor(s => s.Path).Cascade(CascadeMode.Stop)
                .IsValidPath()
                .SetValidator(rootFolderValidator)
                .SetValidator(mappedNetworkDriveValidator)
                .SetValidator(seriesPathValidator)
                .SetValidator(seriesAncestorValidator)
                .SetValidator(systemFolderValidator)
                .When(s => s.Path.IsNotNullOrWhiteSpace());

            PostValidator.RuleFor(s => s.Path).Cascade(CascadeMode.Stop)
                .NotEmpty()
                .IsValidPath()
                .When(s => s.RootFolderPath.IsNullOrWhiteSpace());
            PostValidator.RuleFor(s => s.RootFolderPath).Cascade(CascadeMode.Stop)
                .NotEmpty()
                .IsValidPath()
                .SetValidator(rootFolderExistsValidator)
                .SetValidator(seriesFolderAsRootFolderValidator)
                .When(s => s.Path.IsNullOrWhiteSpace());

            PutValidator.RuleFor(s => s.Path).Cascade(CascadeMode.Stop)
                .NotEmpty()
                .IsValidPath();

            SharedValidator.RuleFor(s => s.QualityProfileId).Cascade(CascadeMode.Stop)
                .ValidId()
                .SetValidator(qualityProfileExistsValidator);

            PostValidator.RuleFor(s => s.Title).NotEmpty();
            PostValidator.RuleFor(s => s.IgdbId).GreaterThan(0).SetValidator(seriesExistsValidator);
        }

        [HttpGet]
        [Produces("application/json")]
        public List<GameResource> AllSeries(int? igdbId, bool includePlatformImages = false)
        {
            var seriesStats = _gameStatisticsService.GameStatistics();
            var gameResources = new List<GameResource>();

            if (igdbId.HasValue)
            {
                gameResources.AddIfNotNull(_gameService.FindByIgdbId(igdbId.Value).ToResource(includePlatformImages));
            }
            else
            {
                gameResources.AddRange(_gameService.GetAllGames().Select(s => s.ToResource(includePlatformImages)));
            }

            MapCoversToLocal(gameResources.ToArray());
            LinkGameStatistics(gameResources, seriesStats.ToDictionary(x => x.GameId));
            PopulateAlternateTitles(gameResources);
            gameResources.ForEach(LinkRootFolderPath);

            return gameResources;
        }

        [NonAction]
        public override ActionResult<GameResource> GetResourceByIdWithErrorHandler(int id)
        {
            return base.GetResourceByIdWithErrorHandler(id);
        }

        [RestGetById]
        [Produces("application/json")]
        public ActionResult<GameResource> GetResourceByIdWithErrorHandler(int id, [FromQuery] bool includePlatformImages = false)
        {
            try
            {
                return GetGameResourceById(id, includePlatformImages);
            }
            catch (ModelNotFoundException)
            {
                return NotFound();
            }
        }

        protected override GameResource GetResourceById(int id)
        {
            var includePlatformImages = Request?.GetBooleanQueryParameter("includePlatformImages", false) ?? false;

            // Parse IncludeImages and use it
            return GetGameResourceById(id, includePlatformImages);
        }

        private GameResource GetGameResourceById(int id, bool includePlatformImages = false)
        {
            var game = _gameService.GetGame(id);

            // Parse IncludeImages and use it
            return GetGameResource(game, includePlatformImages);
        }

        [RestPostById]
        [Consumes("application/json")]
        [Produces("application/json")]
        public ActionResult<GameResource> AddGame([FromBody] GameResource gameResource)
        {
            var game = _addGameService.AddGame(gameResource.ToModel());

            return Created(game.Id);
        }

        [RestPutById]
        [Consumes("application/json")]
        [Produces("application/json")]
        public ActionResult<GameResource> UpdateSeries([FromBody] GameResource gameResource, [FromQuery] bool moveFiles = false)
        {
            var game = _gameService.GetGame(gameResource.Id);

            if (moveFiles)
            {
                var sourcePath = game.Path;
                var destinationPath = gameResource.Path;

                _commandQueueManager.Push(new MoveGameCommand
                {
                    GameId = game.Id,
                    SourcePath = sourcePath,
                    DestinationPath = destinationPath
                },
                    trigger: CommandTrigger.Manual);
            }

            var model = gameResource.ToModel(game);

            _gameService.UpdateSeries(model);

            BroadcastResourceChange(ModelAction.Updated, gameResource);

            return Accepted(gameResource.Id);
        }

        [RestDeleteById]
        public void DeleteGame(int id, bool deleteFiles = false, bool addImportListExclusion = false)
        {
            _gameService.DeleteGame(new List<int> { id }, deleteFiles, addImportListExclusion);
        }

        private GameResource GetGameResource(Romarr.Core.Games.Game game, bool includePlatformImages)
        {
            if (game == null)
            {
                return null;
            }

            var resource = game.ToResource(includePlatformImages);
            MapCoversToLocal(resource);
            FetchAndLinkGameStatistics(resource);
            PopulateAlternateTitles(resource);
            LinkRootFolderPath(resource);

            return resource;
        }

        private void MapCoversToLocal(params GameResource[] game)
        {
            foreach (var gameResource in game)
            {
                _coverMapper.ConvertToLocalUrls(gameResource.Id, gameResource.Images);
            }
        }

        private void FetchAndLinkGameStatistics(GameResource resource)
        {
            LinkGameStatistics(resource, _gameStatisticsService.GameStatistics(resource.Id));
        }

        private void LinkGameStatistics(List<GameResource> resources, Dictionary<int, GameStatistics> gameStatistics)
        {
            foreach (var game in resources)
            {
                if (gameStatistics.TryGetValue(game.Id, out var stats))
                {
                    LinkGameStatistics(game, stats);
                }
            }
        }

        private void LinkGameStatistics(GameResource resource, GameStatistics gameStatistics)
        {
            // Only set last aired from statistics if it's missing from the game itself
            resource.LastAired ??= gameStatistics.LastAired;

            resource.PreviousAiring = gameStatistics.PreviousAiring;
            resource.NextAiring = gameStatistics.NextAiring;
            resource.Statistics = gameStatistics.ToResource(resource.Platforms);

            if (gameStatistics.PlatformStatistics != null)
            {
                foreach (var platform in resource.Platforms)
                {
                    platform.Statistics = gameStatistics.PlatformStatistics.SingleOrDefault(s => s.PlatformNumber == platform.PlatformNumber).ToResource();
                }
            }
        }

        private void PopulateAlternateTitles(List<GameResource> resources)
        {
            foreach (var resource in resources)
            {
                PopulateAlternateTitles(resource);
            }
        }

        private void PopulateAlternateTitles(GameResource resource)
        {
            var mappings = _sceneMappingService.FindByIgdbId(resource.IgdbId);

            if (mappings == null)
            {
                return;
            }

            resource.AlternateTitles = mappings.ConvertAll(AlternateTitleResourceMapper.ToResource);
        }

        private void LinkRootFolderPath(GameResource resource)
        {
            resource.RootFolderPath = _rootFolderService.GetBestRootFolderPath(resource.Path);
        }

        [NonAction]
        public void Handle(FileImportedEvent message)
        {
            BroadcastResourceChange(ModelAction.Updated, message.ImportedGameFile.GameId);
        }

        [NonAction]
        public void Handle(RomFileDeletedEvent message)
        {
            if (message.Reason == DeleteMediaFileReason.Upgrade)
            {
                return;
            }

            BroadcastResourceChange(ModelAction.Updated, message.RomFile.GameId);
        }

        [NonAction]
        public void Handle(GameUpdatedEvent message)
        {
            BroadcastResourceChange(ModelAction.Updated, message.Game.Id);
        }

        [NonAction]
        public void Handle(GameEditedEvent message)
        {
            var resource = GetGameResource(message.Game, false);
            resource.GameFilesChanged = message.GameFilesChanged;
            BroadcastResourceChange(ModelAction.Updated, resource);
        }

        [NonAction]
        public void Handle(GameDeletedEvent message)
        {
            foreach (var game in message.Game)
            {
                BroadcastResourceChange(ModelAction.Deleted, GetGameResource(game, false));
            }
        }

        [NonAction]
        public void Handle(SeriesRenamedEvent message)
        {
            BroadcastResourceChange(ModelAction.Updated, message.Game.Id);
        }

        [NonAction]
        public void Handle(SeriesBulkEditedEvent message)
        {
            foreach (var game in message.Game)
            {
                BroadcastResourceChange(ModelAction.Updated, GetGameResource(game, false));
            }
        }

        [NonAction]
        public void Handle(MediaCoversUpdatedEvent message)
        {
            if (message.Updated)
            {
                BroadcastResourceChange(ModelAction.Updated, message.Game.Id);
            }
        }
    }
}
