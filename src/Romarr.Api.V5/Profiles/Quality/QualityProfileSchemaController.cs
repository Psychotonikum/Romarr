using Microsoft.AspNetCore.Mvc;
using Romarr.Core.Profiles.Qualities;
using Romarr.Http;

namespace Romarr.Api.V5.Profiles.Quality
{
    [V5ApiController("qualityprofile/schema")]
    public class QualityProfileSchemaController : Controller
    {
        private readonly IQualityProfileService _profileService;

        public QualityProfileSchemaController(IQualityProfileService profileService)
        {
            _profileService = profileService;
        }

        [HttpGet]
        public QualityProfileResource GetSchema()
        {
            var qualityProfile = _profileService.GetDefaultProfile(string.Empty);

            return qualityProfile.ToResource();
        }
    }
}
