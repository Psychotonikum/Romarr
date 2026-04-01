using Romarr.Core.Messaging.Commands;

namespace Romarr.Core.Instrumentation.Commands
{
    public class ClearLogCommand : Command
    {
        public override bool SendUpdatesToClient => true;
    }
}
