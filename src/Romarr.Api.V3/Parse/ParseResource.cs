using System.Collections.Generic;
using Romarr.Core.Languages;
using Romarr.Core.Parser.Model;
using Romarr.Api.V3.CustomFormats;
using Romarr.Api.V3.Roms;
using Romarr.Api.V3.Game;
using Romarr.Http.REST;

namespace Romarr.Api.V3.Parse
{
    public class ParseResource : RestResource
    {
        public string Title { get; set; }
        public ParsedRomInfo ParsedRomInfo { get; set; }
        public GameResource Game { get; set; }
        public List<RomResource> Roms { get; set; }
        public List<Language> Languages { get; set; }
        public List<CustomFormatResource> CustomFormats { get; set; }
        public int CustomFormatScore { get; set; }
    }
}
