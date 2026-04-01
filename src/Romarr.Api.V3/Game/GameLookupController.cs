using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Romarr.Core.MediaCover;
using Romarr.Core.MetadataSource;
using Romarr.Core.Organizer;
using Romarr.Core.GameStats;
using Romarr.Http;

namespace Romarr.Api.V3.Game
{
    [V3ApiController("game/lookup")]
    public class GameLookupController : Controller
    {
        private readonly ISearchForNewGame _searchProxy;
        private readonly IBuildFileNames _fileNameBuilder;
        private readonly IMapCoversToLocal _coverMapper;

        public GameLookupController(ISearchForNewGame searchProxy, IBuildFileNames fileNameBuilder, IMapCoversToLocal coverMapper)
        {
            _searchProxy = searchProxy;
            _fileNameBuilder = fileNameBuilder;
            _coverMapper = coverMapper;
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

                yield return resource;
            }
        }
    }
}
