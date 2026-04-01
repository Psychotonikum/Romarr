using Romarr.Core.Messaging.Commands;

namespace Romarr.Core.Blocklisting
{
    public class ClearBlocklistCommand : Command
    {
        public override bool SendUpdatesToClient => true;
    }
}
