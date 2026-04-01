using System.Collections.Generic;
using Romarr.Common.Messaging;
using Romarr.Core.MediaFiles;
using Romarr.Core.Parser.Model;
using Romarr.Core.Games;

namespace Romarr.Core.Download
{
    public class UntrackedDownloadCompletedEvent : IEvent
    {
        public Game Game { get; private set; }
        public List<Rom> Roms { get; private set; }
        public List<RomFile> RomFiles { get; private set; }
        public ParsedRomInfo ParsedRomInfo { get; private set; }
        public string SourcePath { get; private set; }

        public UntrackedDownloadCompletedEvent(Game game, List<Rom> roms, List<RomFile> romFiles, ParsedRomInfo parsedRomInfo, string sourcePath)
        {
            Game = game;
            Roms = roms;
            RomFiles = romFiles;
            ParsedRomInfo = parsedRomInfo;
            SourcePath = sourcePath;
        }
    }
}
