using Romarr.Core.Messaging.Commands;

namespace Romarr.Core.Configuration
{
    public class ResetApiKeyCommand : Command
    {
        public override bool SendUpdatesToClient => true;
    }
}
