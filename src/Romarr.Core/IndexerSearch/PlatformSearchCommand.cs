using Romarr.Core.Messaging.Commands;

namespace Romarr.Core.IndexerSearch
{
    public class PlatformSearchCommand : Command
    {
        public int GameId { get; set; }
        public int PlatformNumber { get; set; }

        public override bool SendUpdatesToClient => true;
    }
}
