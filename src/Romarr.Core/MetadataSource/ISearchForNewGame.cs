using System.Collections.Generic;
using Romarr.Core.Games;

namespace Romarr.Core.MetadataSource
{
    public interface ISearchForNewGame
    {
        List<Game> SearchForNewGame(string title);
        List<Game> SearchForNewGameByImdbId(string imdbId);
        List<Game> SearchForNewGameByAniListId(int aniListId);
        List<Game> SearchForNewGameByTmdbId(int tmdbId);
        List<Game> SearchForNewGameByMyAnimeListId(int malId);
    }
}
