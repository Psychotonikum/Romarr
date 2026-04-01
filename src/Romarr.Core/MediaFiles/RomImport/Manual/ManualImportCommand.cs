using System.Collections.Generic;
using Romarr.Core.Messaging.Commands;

namespace Romarr.Core.MediaFiles.GameFileImport.Manual
{
    public class ManualImportCommand : Command
    {
        public List<ManualImportFile> Files { get; set; }

        public override bool SendUpdatesToClient => true;
        public override bool RequiresDiskAccess => true;

        public ImportMode ImportMode { get; set; }
    }
}
