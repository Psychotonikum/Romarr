using Romarr.Core.Messaging.Commands;

namespace Romarr.Core.Instrumentation.Commands
{
    public class DeleteUpdateLogFilesCommand : Command
    {
        public override bool SendUpdatesToClient => true;
    }
}
