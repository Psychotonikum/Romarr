using Romarr.Core.Download;
using Romarr.Core.Parser.Model;

namespace Romarr.Core.MediaFiles.GameFileImport.Aggregation.Aggregators
{
    public interface IAggregateLocalGameFile
    {
        int Order { get; }
        LocalGameFile Aggregate(LocalGameFile localRom, DownloadClientItem downloadClientItem);
    }
}
