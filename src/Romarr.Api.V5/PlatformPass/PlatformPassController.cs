using Microsoft.AspNetCore.Mvc;
using Romarr.Core.Games;
using Romarr.Http;

namespace Romarr.Api.V5.PlatformPass;

[V5ApiController]
public class PlatformPassController : Controller
{
    private readonly IGameService _gameService;
    private readonly IFileMonitoredService _romMonitoredService;

    public PlatformPassController(IGameService seriesService, IFileMonitoredService gameFileMonitoredService)
    {
        _gameService = seriesService;
        _romMonitoredService = gameFileMonitoredService;
    }

    [HttpPost]
    [Consumes("application/json")]
    public IActionResult UpdateAll([FromBody] PlatformPassResource resource)
    {
        var gamesToUpdate = _gameService.GetGame(resource.Game.Select(s => s.Id));

        foreach (var s in resource.Game)
        {
            var game = gamesToUpdate.Single(c => c.Id == s.Id);

            if (s.Monitored.HasValue)
            {
                game.Monitored = s.Monitored.Value;
            }

            if (s.Platforms.Any())
            {
                foreach (var seriesPlatform in game.Platforms)
                {
                    var platform = s.Platforms.FirstOrDefault(c => c.PlatformNumber == seriesPlatform.PlatformNumber);

                    if (platform != null)
                    {
                        seriesPlatform.Monitored = platform.Monitored;
                    }
                }
            }

            if (resource.MonitoringOptions != null && resource.MonitoringOptions.Monitor == MonitorTypes.None)
            {
                game.Monitored = false;
            }

            _romMonitoredService.SetGameFileMonitoredStatus(game, resource.MonitoringOptions.ToModel());
        }

        return NoContent();
    }
}
