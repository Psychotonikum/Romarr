using System.Collections.Generic;
using Romarr.Core.Extras.Files;

namespace Romarr.Core.Extras
{
    public class ImportExistingExtraFileFilterResult<TExtraFile>
        where TExtraFile : ExtraFile, new()
    {
        public ImportExistingExtraFileFilterResult(List<TExtraFile> previouslyImported, List<string> filesOnDisk)
        {
            PreviouslyImported = previouslyImported;
            FilesOnDisk = filesOnDisk;
        }

        public List<TExtraFile> PreviouslyImported { get; set; }
        public List<string> FilesOnDisk { get; set; }
    }
}
