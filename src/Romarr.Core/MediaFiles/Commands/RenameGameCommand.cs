using System.Collections.Generic;
using Romarr.Core.Messaging.Commands;

namespace Romarr.Core.MediaFiles.Commands
{
    public class RenameGameCommand : Command
    {
        public List<int> GameIds { get; set; }

        public override bool SendUpdatesToClient => true;
        public override bool RequiresDiskAccess => true;

        public RenameGameCommand()
        {
            GameIds = new List<int>();
        }
    }
}
