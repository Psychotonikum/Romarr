using Romarr.Core.Download;
using Romarr.Core.Parser.Model;

namespace Romarr.Core.MediaFiles.GameFileImport.Aggregation.Aggregators.Augmenters.Quality
{
    public interface IAugmentQuality
    {
        int Order { get; }
        string Name { get; }
        AugmentQualityResult AugmentQuality(LocalGameFile localRom, DownloadClientItem downloadClientItem);
    }
}
