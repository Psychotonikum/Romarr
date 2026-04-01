using System.Collections.Generic;
using Romarr.Core.Languages;
using Romarr.Core.Qualities;

namespace Romarr.Api.V3.RomFiles
{
    public class RomFileListResource
    {
        public List<int> RomFileIds { get; set; }
        public List<Language> Languages { get; set; }
        public QualityModel Quality { get; set; }
        public string SceneName { get; set; }
        public string ReleaseGroup { get; set; }
    }
}
