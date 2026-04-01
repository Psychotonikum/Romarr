using System.Collections.Generic;
using Romarr.Core.MediaFiles;
using Romarr.Core.Games;

namespace Romarr.Core.Organizer
{
    public class SampleResult
    {
        public string FileName { get; set; }
        public Game Game { get; set; }
        public List<Rom> Roms { get; set; }
        public RomFile RomFile { get; set; }
    }
}
