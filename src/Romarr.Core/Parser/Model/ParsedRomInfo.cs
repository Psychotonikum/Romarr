using System;
using System.Collections.Generic;
using System.Linq;
using Romarr.Common.Extensions;
using Romarr.Core.Languages;
using Romarr.Core.Qualities;

namespace Romarr.Core.Parser.Model
{
    public class ParsedRomInfo
    {
        public string ReleaseTitle { get; set; }
        public string GameTitle { get; set; }
        public GameTitleInfo GameTitleInfo { get; set; }
        public QualityModel Quality { get; set; }
        public int PlatformNumber { get; set; }
        public int[] RomNumbers { get; set; }
        public int[] AbsoluteRomNumbers { get; set; }
        public decimal[] SpecialAbsoluteRomNumbers { get; set; }
        public string AirDate { get; set; }
        public List<Language> Languages { get; set; }
        public bool FullPlatform { get; set; }
        public bool IsPartialPlatform { get; set; }
        public bool IsMultiPlatform { get; set; }
        public bool IsPlatformExtra { get; set; }
        public bool IsSplitGameFile { get; set; }
        public bool IsMiniSeries { get; set; }
        public bool Special { get; set; }
        public string ReleaseGroup { get; set; }
        public string ReleaseHash { get; set; }
        public int PlatformPart { get; set; }
        public string ReleaseTokens { get; set; }
        public int? DailyPart { get; set; }

        public ParsedRomInfo()
        {
            RomNumbers = Array.Empty<int>();
            AbsoluteRomNumbers = Array.Empty<int>();
            SpecialAbsoluteRomNumbers = Array.Empty<decimal>();
            Languages = new List<Language>();
        }

        public bool IsDaily
        {
            get
            {
                return !string.IsNullOrWhiteSpace(AirDate);
            }

            private set
            {
            }
        }

        public bool IsAbsoluteNumbering
        {
            get
            {
                return AbsoluteRomNumbers.Any();
            }

            private set
            {
            }
        }

        public bool IsPossibleSpecialGameFile
        {
            get
            {
                return ((AirDate.IsNullOrWhiteSpace() &&
                       GameTitle.IsNullOrWhiteSpace() &&
                       (RomNumbers.Length == 0 || PlatformNumber == 0)) || (!GameTitle.IsNullOrWhiteSpace() && Special)) ||
                       (RomNumbers.Length == 1 && RomNumbers[0] == 0);
            }

            private set
            {
            }
        }

        public bool IsPossibleScenePlatformSpecial
        {
            get
            {
                return PlatformNumber != 0 && RomNumbers.Length == 1 && RomNumbers[0] == 0;
            }

            private set
            {
            }
        }

        public ReleaseType ReleaseType
        {
            get
            {
                if (RomNumbers.Length > 1 || AbsoluteRomNumbers.Length > 1)
                {
                    return Model.ReleaseType.MultiGameFile;
                }

                if (RomNumbers.Length == 1 || AbsoluteRomNumbers.Length == 1)
                {
                    return Model.ReleaseType.SingleGameFile;
                }

                if (FullPlatform)
                {
                    return Model.ReleaseType.PlatformPack;
                }

                return Model.ReleaseType.Unknown;
            }
        }

        public override string ToString()
        {
            var gameFileString = "[Unknown Rom]";

            if (IsDaily && RomNumbers.Empty())
            {
                gameFileString = string.Format("{0}", AirDate);
            }
            else if (FullPlatform)
            {
                gameFileString = string.Format("Platform {0:00}", PlatformNumber);
            }
            else if (RomNumbers != null && RomNumbers.Any())
            {
                gameFileString = string.Format("S{0:00}E{1}", PlatformNumber, string.Join("-", RomNumbers.Select(c => c.ToString("00"))));
            }
            else if (AbsoluteRomNumbers != null && AbsoluteRomNumbers.Any())
            {
                gameFileString = string.Format("{0}", string.Join("-", AbsoluteRomNumbers.Select(c => c.ToString("000"))));
            }
            else if (Special)
            {
                if (PlatformNumber != 0)
                {
                    gameFileString = string.Format("[Unknown Platform {0:00} Special]", PlatformNumber);
                }
                else
                {
                    gameFileString = "[Unknown Special]";
                }
            }

            return string.Format("{0} - {1} {2}", GameTitle, gameFileString, Quality);
        }
    }
}
