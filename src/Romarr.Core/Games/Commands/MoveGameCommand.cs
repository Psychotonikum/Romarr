using Romarr.Core.Messaging.Commands;

namespace Romarr.Core.Games.Commands
{
    public class MoveGameCommand : Command
    {
        public int GameId { get; set; }
        public string SourcePath { get; set; }
        public string DestinationPath { get; set; }

        public override bool SendUpdatesToClient => true;
        public override bool RequiresDiskAccess => true;
    }
}
