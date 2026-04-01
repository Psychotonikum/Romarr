using Romarr.Core.Messaging.Commands;

namespace Romarr.Core.MediaFiles.Commands
{
    public class RescanGameCommand : Command
    {
        public int? GameId { get; set; }

        public override bool SendUpdatesToClient => true;

        public RescanGameCommand()
        {
        }

        public RescanGameCommand(int gameId)
        {
            GameId = gameId;
        }
    }
}
