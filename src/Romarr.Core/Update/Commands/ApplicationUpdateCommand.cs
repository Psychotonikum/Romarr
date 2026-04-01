using Romarr.Core.Messaging.Commands;

namespace Romarr.Core.Update.Commands
{
    public class ApplicationUpdateCommand : Command
    {
        public bool InstallMajorUpdate { get; set; }
        public override bool SendUpdatesToClient => true;
        public override bool IsExclusive => true;
    }
}
