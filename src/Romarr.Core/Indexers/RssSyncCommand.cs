using Romarr.Core.Messaging.Commands;

namespace Romarr.Core.Indexers
{
    public class RssSyncCommand : Command
    {
        public override bool SendUpdatesToClient => true;
        public override bool IsLongRunning => true;
    }
}
