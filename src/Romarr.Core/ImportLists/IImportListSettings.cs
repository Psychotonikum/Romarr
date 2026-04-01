using Romarr.Core.ThingiProvider;

namespace Romarr.Core.ImportLists
{
    public interface IImportListSettings : IProviderConfig
    {
        string BaseUrl { get; set; }
    }
}
