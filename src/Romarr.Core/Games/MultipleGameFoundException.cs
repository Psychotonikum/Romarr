using System.Collections.Generic;
using Romarr.Common.Exceptions;

namespace Romarr.Core.Games
{
    public class MultipleSeriesFoundException : RomarrException
    {
        public List<Game> Game { get; set; }

        public MultipleSeriesFoundException(List<Game> game, string message, params object[] args)
            : base(message, args)
        {
            Game = game;
        }
    }
}
