using Romarr.Common.Exceptions;

namespace Romarr.Core.Exceptions
{
    public class GameNotFoundException : RomarrException
    {
        public int IgdbGameId { get; set; }

        public GameNotFoundException(int igdbGameId)
            : base(string.Format("Game with igdbid {0} was not found, it may have been removed from TheIGDB.", igdbGameId))
        {
            IgdbGameId = igdbGameId;
        }

        public GameNotFoundException(int igdbGameId, string message, params object[] args)
            : base(message, args)
        {
            IgdbGameId = igdbGameId;
        }

        public GameNotFoundException(int igdbGameId, string message)
            : base(message)
        {
            IgdbGameId = igdbGameId;
        }
    }
}
