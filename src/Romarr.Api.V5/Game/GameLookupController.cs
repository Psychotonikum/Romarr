using Microsoft.AspNetCore.Mvc;
using Romarr.Core.ImportLists.Exclusions;
using Romarr.Core.MediaCover;
using Romarr.Core.MetadataSource;
using Romarr.Core.Organizer;
using Romarr.Core.GameStats;
using Romarr.Http;

namespace Romarr.Api.V5.Game;

[V5ApiController("game/lookup")]
public class GameLookupController : Controller
{
    private readonly ISearchForNewGame _searchProxy;
    private readonly IBuildFileNames _fileNameBuilder;
    private readonly IMapCoversToLocal _coverMapper;
    private readonly IImportListExclusionService _importListExclusionService;

    public GameLookupController(ISearchForNewGame searchProxy, IBuildFileNames fileNameBuilder, IMapCoversToLocal coverMapper,  IImportListExclusionService importListExclusionService)
    {
        _searchProxy = searchProxy;
        _fileNameBuilder = fileNameBuilder;
        _coverMapper = coverMapper;
        _importListExclusionService = importListExclusionService;
    }

    [HttpGet]
    public IEnumerable<GameResource> Search([FromQuery] string term)
    {
        var tvDbResults = _searchProxy.SearchForNewGame(term);
        return MapToResource(tvDbResults);
    }

    private IEnumerable<GameResource> MapToResource(IEnumerable<Romarr.Core.Games.Game> game)
    {
        foreach (var currentSeries in game)
        {
            var resource = currentSeries.ToResource();

            _coverMapper.ConvertToLocalUrls(resource.Id, resource.Images);

            var poster = currentSeries.Images.FirstOrDefault(c => c.CoverType == MediaCoverTypes.Poster);

            if (poster != null)
            {
                resource.RemotePoster = poster.RemoteUrl;
            }

            resource.Folder = _fileNameBuilder.GetGameFolder(currentSeries);
            resource.Statistics = new GameStatistics().ToResource(resource.Platforms);
            resource.IsExcluded = _importListExclusionService.FindByIgdbId(currentSeries.IgdbId) is not null;

            yield return resource;
        }
    }
}
