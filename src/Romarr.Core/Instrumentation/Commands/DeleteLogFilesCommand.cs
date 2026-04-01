using Romarr.Core.Messaging.Commands;

namespace Romarr.Core.Instrumentation.Commands
{
    public class DeleteLogFilesCommand : Command
    {
        public override bool SendUpdatesToClient => true;
    }
}
