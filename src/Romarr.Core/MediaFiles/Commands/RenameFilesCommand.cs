using System.Collections.Generic;
using Romarr.Core.Messaging.Commands;

namespace Romarr.Core.MediaFiles.Commands
{
    public class RenameFilesCommand : Command
    {
        public int GameId { get; set; }
        public List<int> Files { get; set; }

        public override bool SendUpdatesToClient => true;
        public override bool RequiresDiskAccess => true;

        public RenameFilesCommand()
        {
        }

        public RenameFilesCommand(int gameId, List<int> files)
        {
            GameId = gameId;
            Files = files;
        }
    }
}
