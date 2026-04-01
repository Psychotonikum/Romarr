using Microsoft.AspNetCore.Mvc;
using Romarr.Core.Organizer;
using Romarr.Core.Games;
using Romarr.Http;

namespace Romarr.Api.V5.Game;

[V5ApiController("game")]
public class GameFolderController : Controller
{
    private readonly IGameService _gameService;
    private readonly IBuildFileNames _fileNameBuilder;

    public GameFolderController(IGameService seriesService, IBuildFileNames fileNameBuilder)
    {
        _gameService = seriesService;
        _fileNameBuilder = fileNameBuilder;
    }

    [HttpGet("{id}/folder")]
    [Produces("application/json")]
    public object GetFolder([FromRoute] int id)
    {
        var game = _gameService.GetGame(id);
        var folder = _fileNameBuilder.GetGameFolder(game);

        return new
        {
            folder
        };
    }
}
