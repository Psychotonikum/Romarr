using System.Collections.Generic;
using Romarr.Core.Extras.Files;
using Romarr.Core.Games;

namespace Romarr.Core.Extras
{
    public interface IImportExistingExtraFiles
    {
        int Order { get; }
        IEnumerable<ExtraFile> ProcessFiles(Game game, List<string> filesOnDisk, List<string> importedFiles, string fileNameBeforeRename);
    }
}
