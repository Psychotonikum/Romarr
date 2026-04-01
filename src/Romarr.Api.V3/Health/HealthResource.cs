using System.Collections.Generic;
using System.Linq;
using Romarr.Core.HealthCheck;
using Romarr.Http.REST;

namespace Romarr.Api.V3.Health
{
    public class HealthResource : RestResource
    {
        public string Source { get; set; }
        public HealthCheckResult Type { get; set; }
        public string Message { get; set; }
        public string WikiUrl { get; set; }
    }

    public static class HealthResourceMapper
    {
        public static HealthResource ToResource(this HealthCheck model)
        {
            if (model == null)
            {
                return null;
            }

            return new HealthResource
            {
                Id = model.Id,
                Source = model.Source.Name,
                Type = model.Type,
                Message = model.Message,
                WikiUrl = model.WikiUrl.FullUri
            };
        }

        public static List<HealthResource> ToResource(this IEnumerable<HealthCheck> models)
        {
            return models.Select(ToResource).ToList();
        }
    }
}
