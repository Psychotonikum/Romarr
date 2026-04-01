using System;
using System.Collections.Generic;
using Romarr.Core.Games;

namespace Romarr.Core.MetadataSource
{
    public interface IProvideGameInfo
    {
        Tuple<Game, List<Rom>> GetGameInfo(int igdbGameId);
    }
}
