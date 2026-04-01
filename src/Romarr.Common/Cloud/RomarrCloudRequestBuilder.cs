using Romarr.Common.Http;

namespace Romarr.Common.Cloud
{
    public interface IRomarrCloudRequestBuilder
    {
        IHttpRequestBuilderFactory Services { get; }
        IHttpRequestBuilderFactory SkyHookIgdb { get; }
    }

    public class RomarrCloudRequestBuilder : IRomarrCloudRequestBuilder
    {
        public RomarrCloudRequestBuilder()
        {
            Services = new HttpRequestBuilder("https://services.romarr.tv/v1/")
                .CreateFactory();

            SkyHookIgdb = new HttpRequestBuilder("https://skyhook.romarr.tv/v1/igdb/{route}/{language}/")
                .SetSegment("language", "en")
                .CreateFactory();
        }

        public IHttpRequestBuilderFactory Services { get; }

        public IHttpRequestBuilderFactory SkyHookIgdb { get; }
    }
}
