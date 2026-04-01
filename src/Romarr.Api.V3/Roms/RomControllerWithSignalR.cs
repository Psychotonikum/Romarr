using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Romarr.Core.CustomFormats;
using Romarr.Core.Datastore.Events;
using Romarr.Core.DecisionEngine.Specifications;
using Romarr.Core.Download;
using Romarr.Core.MediaFiles.Events;
using Romarr.Core.Messaging.Events;
using Romarr.Core.Games;
using Romarr.SignalR;
using Romarr.Api.V3.RomFiles;
using Romarr.Api.V3.Game;
using Romarr.Http.REST;

namespace Romarr.Api.V3.Roms
{
    public abstract class RomControllerWithSignalR : RestControllerWithSignalR<RomResource, Rom>,
                                                         IHandle<FileGrabbedEvent>,
                                                         IHandle<FileImportedEvent>,
                                                         IHandle<RomFileDeletedEvent>
    {
        protected readonly IRomService _romService;
        protected readonly IGameService _gameService;
        protected readonly IUpgradableSpecification _upgradableSpecification;
        protected readonly ICustomFormatCalculationService _formatCalculator;

        protected RomControllerWithSignalR(IRomService gameFileService,
                                           IGameService seriesService,
                                           IUpgradableSpecification upgradableSpecification,
                                           ICustomFormatCalculationService formatCalculator,
                                           IBroadcastSignalRMessage signalRBroadcaster)
            : base(signalRBroadcaster)
        {
            _romService = gameFileService;
            _gameService = seriesService;
            _upgradableSpecification = upgradableSpecification;
            _formatCalculator = formatCalculator;
        }

        protected RomControllerWithSignalR(IRomService gameFileService,
                                           IGameService seriesService,
                                           IUpgradableSpecification upgradableSpecification,
                                           ICustomFormatCalculationService formatCalculator,
                                           IBroadcastSignalRMessage signalRBroadcaster,
                                           string resource)
            : base(signalRBroadcaster)
        {
            _romService = gameFileService;
            _gameService = seriesService;
            _upgradableSpecification = upgradableSpecification;
            _formatCalculator = formatCalculator;
        }

        protected override RomResource GetResourceById(int id)
        {
            var rom = _romService.GetGameFile(id);
            var resource = MapToResource(rom, true, true, true);
            return resource;
        }

        protected RomResource MapToResource(Rom rom, bool includeSeries, bool includeRomFile, bool includeImages)
        {
            var resource = rom.ToResource();

            if (includeSeries || includeRomFile || includeImages)
            {
                var game = rom.Game ?? _gameService.GetGame(rom.GameId);

                if (includeSeries)
                {
                    resource.Game = game.ToResource();
                }

                if (includeRomFile && rom.RomFileId != 0)
                {
                    resource.RomFile = rom.RomFile.Value.ToResource(game, _upgradableSpecification, _formatCalculator);
                }

                if (includeImages)
                {
                    resource.Images = rom.Images;
                }
            }

            return resource;
        }

        protected List<RomResource> MapToResource(List<Rom> roms, bool includeSeries, bool includeRomFile, bool includeImages)
        {
            var result = roms.ToResource();

            if (includeSeries || includeRomFile || includeImages)
            {
                var seriesDict = new Dictionary<int, Romarr.Core.Games.Game>();
                for (var i = 0; i < roms.Count; i++)
                {
                    var rom = roms[i];
                    var resource = result[i];

                    var game = rom.Game ?? seriesDict.GetValueOrDefault(roms[i].GameId) ?? _gameService.GetGame(roms[i].GameId);
                    seriesDict[game.Id] = game;

                    if (includeSeries)
                    {
                        resource.Game = game.ToResource();
                    }

                    if (includeRomFile && rom.RomFileId != 0)
                    {
                        resource.RomFile = rom.RomFile.Value.ToResource(game, _upgradableSpecification, _formatCalculator);
                    }

                    if (includeImages)
                    {
                        resource.Images = rom.Images;
                    }
                }
            }

            return result;
        }

        [NonAction]
        public void Handle(FileGrabbedEvent message)
        {
            foreach (var rom in message.Rom.Roms)
            {
                var resource = rom.ToResource();
                resource.Grabbed = true;

                BroadcastResourceChange(ModelAction.Updated, resource);
            }
        }

        [NonAction]
        public void Handle(FileImportedEvent message)
        {
            foreach (var rom in message.RomInfo.Roms)
            {
                BroadcastResourceChange(ModelAction.Updated, rom.Id);
            }
        }

        [NonAction]
        public void Handle(RomFileDeletedEvent message)
        {
            foreach (var rom in message.RomFile.Roms.Value)
            {
                BroadcastResourceChange(ModelAction.Updated, rom.Id);
            }
        }
    }
}
