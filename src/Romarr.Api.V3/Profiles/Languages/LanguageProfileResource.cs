using System.Collections.Generic;
using Romarr.Http.REST;

namespace Romarr.Api.V3.Profiles.Languages
{
    public class LanguageProfileResource : RestResource
    {
        public string Name { get; set; }
        public bool UpgradeAllowed { get; set; }
        public Romarr.Core.Languages.Language Cutoff { get; set; }
        public List<LanguageProfileItemResource> Languages { get; set; }
    }

    public class LanguageProfileItemResource : RestResource
    {
        public Romarr.Core.Languages.Language Language { get; set; }
        public bool Allowed { get; set; }
    }
}
