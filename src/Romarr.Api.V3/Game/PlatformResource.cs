using System.Collections.Generic;
using System.Linq;
using Romarr.Core.MediaCover;
using Romarr.Core.Games;

namespace Romarr.Api.V3.Game
{
    public class PlatformResource
    {
        public int PlatformNumber { get; set; }
        public string Title { get; set; }
        public bool Monitored { get; set; }
        public PlatformStatisticsResource Statistics { get; set; }
        public List<MediaCover> Images { get; set; }
    }

    public static class PlatformResourceMapper
    {
        public static PlatformResource ToResource(this Platform model, bool includeImages = false)
        {
            if (model == null)
            {
                return null;
            }

            return new PlatformResource
            {
                PlatformNumber = model.PlatformNumber,
                Title = model.Title,
                Monitored = model.Monitored,
                Images = includeImages ? model.Images : null
            };
        }

        public static Platform ToModel(this PlatformResource resource)
        {
            if (resource == null)
            {
                return null;
            }

            return new Platform
            {
                PlatformNumber = resource.PlatformNumber,
                Title = resource.Title,
                Monitored = resource.Monitored
            };
        }

        public static List<PlatformResource> ToResource(this IEnumerable<Platform> models, bool includeImages = false)
        {
            return models.Select(s => ToResource(s, includeImages)).ToList();
        }

        public static List<Platform> ToModel(this IEnumerable<PlatformResource> resources)
        {
            return resources.Select(ToModel).ToList();
        }
    }
}
