using Romarr.Core.ThingiProvider;

namespace Romarr.Core.MetadataSource.Providers
{
    public interface IMetadataSourceProvider : IProvider
    {
        bool SupportsSearch { get; }
        bool SupportsCalendar { get; }
        bool SupportsMetadataDownload { get; }
    }
}
