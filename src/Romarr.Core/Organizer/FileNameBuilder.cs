using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NLog;
using Romarr.Common.Cache;
using Romarr.Common.Disk;
using Romarr.Common.EnsureThat;
using Romarr.Common.Extensions;
using Romarr.Core.CustomFormats;
using Romarr.Core.MediaFiles;
using Romarr.Core.MediaFiles.MediaInfo;
using Romarr.Core.Qualities;
using Romarr.Core.Games;

namespace Romarr.Core.Organizer
{
    public interface IBuildFileNames
    {
        string BuildFileName(List<Rom> roms, Game game, RomFile romFile, string extension = "", NamingConfig namingConfig = null, List<CustomFormat> customFormats = null);
        string BuildFilePath(List<Rom> roms, Game game, RomFile romFile, string extension, NamingConfig namingConfig = null, List<CustomFormat> customFormats = null);
        string BuildPlatformPath(Game game, int platformNumber);
        string GetGameFolder(Game game, NamingConfig namingConfig = null);
        string GetPlatformFolder(Game game, int platformNumber, NamingConfig namingConfig = null);
        bool RequiresRomTitle(Game game, List<Rom> roms);
        bool RequiresAbsoluteRomNumber();
    }

    public class FileNameBuilder : IBuildFileNames
    {
        private const string MediaInfoVideoDynamicRangeToken = "{MediaInfo FileDynamicRange}";
        private const string MediaInfoVideoDynamicRangeTypeToken = "{MediaInfo FileDynamicRangeType}";

        private readonly INamingConfigService _namingConfigService;
        private readonly IQualityDefinitionService _qualityDefinitionService;
        private readonly IUpdateMediaInfo _mediaInfoUpdater;
        private readonly ICustomFormatCalculationService _formatCalculator;
        private readonly ICached<GameFileFormat[]> _gameFileFormatCache;
        private readonly ICached<AbsoluteGameFileFormat[]> _absoluteGameFileFormatCache;
        private readonly ICached<bool> _requiresRomTitleCache;
        private readonly ICached<bool> _requiresAbsoluteRomNumberCache;
        private readonly ICached<bool> _patternHasRomIdentifierCache;
        private readonly Logger _logger;

        private static readonly Regex TitleRegex = new Regex(@"(?<escaped>\{\{|\}\})|\{(?<prefix>[- ._\[(]*)(?<token>(?:[a-z0-9]+)(?:(?<separator>[- ._]+)(?:[a-z0-9]+))?)(?::(?<customFormat>[ ,a-z0-9+-]+(?<![- ])))?(?<suffix>[- ._)\]]*)\}",
                                                             RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        public static readonly Regex GameFileRegex = new Regex(@"(?<rom>\{rom(?:\:0+)?})",
                                                               RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static readonly Regex PlatformRegex = new Regex(@"(?<platform>\{platform(?:\:0+)?})",
                                                              RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex AbsoluteGameFileRegex = new Regex(@"(?<absolute>\{absolute(?:\:0+)?})",
                                                               RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static readonly Regex PlatformGameFilePatternRegex = new Regex(@"(?<separator>(?<=})[- ._]+?)?(?<platformGameFile>s?{platform(?:\:0+)?}(?<gameFileSeparator>[- ._]?[ex])(?<rom>{rom(?:\:0+)?}))(?<separator>[- ._]+?(?={))?",
                                                                            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static readonly Regex AbsoluteGameFilePatternRegex = new Regex(@"(?<separator>(?<=})[- ._]+?)?(?<absolute>{absolute(?:\:0+)?})(?<separator>[- ._]+?(?={))?",
                                                                    RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static readonly Regex GameTitleRegex = new Regex(@"(?<token>\{(?:Game)(?<separator>[- ._])(Clean)?Title(The)?(Without)?(Year)?(?::(?<customFormat>[0-9-]+))?\})",
                                                                            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex FileNameCleanupRegex = new Regex(@"([- ._])(\1)+", RegexOptions.Compiled);
        private static readonly Regex TrimSeparatorsRegex = new Regex(@"[- ._]+$", RegexOptions.Compiled);

        private static readonly Regex ScenifyRemoveChars = new Regex(@"(?<=\s)(,|<|>|\/|\\|;|:|'|""|\||`|’|~|!|\?|@|$|%|^|\*|-|_|=){1}(?=\s)|('|`|’|:|\?|,)(?=(?:(?:s|m|t|ve|ll|d|re)\s)|\s|$)|(\(|\)|\[|\]|\{|\})", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex ScenifyReplaceChars = new Regex(@"[\/]", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        // TODO: Support Written numbers (One, Two, etc) and Roman Numerals (I, II, III etc)
        private static readonly Regex MultiPartCleanupRegex = new Regex(@"(?:\:?\s?(?:\(\d+\)|(Part|Pt\.?)\s?\d+))$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly char[] RomTitleTrimCharacters = new[] { ' ', '.', '?' };

        private static readonly Regex TitlePrefixRegex = new Regex(@"^(The|An|A) (.*?)((?: *\([^)]+\))*)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex YearRegex = new Regex(@"\(\d{4}\)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex ReservedDeviceNamesRegex = new Regex(@"^(?:aux|com[1-9]|con|lpt[1-9]|nul|prn)\.", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        // generated from https://www.loc.gov/standards/iso639-2/ISO-639-2_utf-8.txt
        public static readonly ImmutableDictionary<string, string> Iso639BTMap = new Dictionary<string, string>
        {
            { "alb", "sqi" },
            { "arm", "hye" },
            { "baq", "eus" },
            { "bur", "mya" },
            { "chi", "zho" },
            { "cze", "ces" },
            { "dut", "nld" },
            { "fre", "fra" },
            { "geo", "kat" },
            { "ger", "deu" },
            { "gre", "ell" },
            { "gsw", "deu" },
            { "ice", "isl" },
            { "mac", "mkd" },
            { "mao", "mri" },
            { "may", "msa" },
            { "per", "fas" },
            { "rum", "ron" },
            { "slo", "slk" },
            { "tib", "bod" },
            { "wel", "cym" }
        }.ToImmutableDictionary();

        public static readonly ImmutableArray<string> BadCharacters = ImmutableArray.Create("\\", "/", "<", ">", "?", "*", "|", "\"");
        public static readonly ImmutableArray<string> GoodCharacters = ImmutableArray.Create("+", "+", "", "", "!", "-", "", "");

        public FileNameBuilder(INamingConfigService namingConfigService,
                               IQualityDefinitionService qualityDefinitionService,
                               ICacheManager cacheManager,
                               IUpdateMediaInfo mediaInfoUpdater,
                               ICustomFormatCalculationService formatCalculator,
                               Logger logger)
        {
            _namingConfigService = namingConfigService;
            _qualityDefinitionService = qualityDefinitionService;
            _mediaInfoUpdater = mediaInfoUpdater;
            _formatCalculator = formatCalculator;
            _gameFileFormatCache = cacheManager.GetCache<GameFileFormat[]>(GetType(), "gameFileFormat");
            _absoluteGameFileFormatCache = cacheManager.GetCache<AbsoluteGameFileFormat[]>(GetType(), "absoluteGameFileFormat");
            _requiresRomTitleCache = cacheManager.GetCache<bool>(GetType(), "requiresRomTitle");
            _requiresAbsoluteRomNumberCache = cacheManager.GetCache<bool>(GetType(), "requiresAbsoluteRomNumber");
            _patternHasRomIdentifierCache = cacheManager.GetCache<bool>(GetType(), "patternHasRomIdentifier");
            _logger = logger;
        }

        private string BuildFileName(List<Rom> roms, Game game, RomFile romFile, string extension, int maxPath, NamingConfig namingConfig = null, List<CustomFormat> customFormats = null)
        {
            if (namingConfig == null)
            {
                namingConfig = _namingConfigService.GetConfig();
            }

            if (!namingConfig.RenameGameFiles)
            {
                return GetOriginalTitle(romFile, true) + extension;
            }

            if (namingConfig.StandardGameFileFormat.IsNullOrWhiteSpace() && game.GameType == GameTypes.Standard)
            {
                throw new NamingFormatException("Standard rom format cannot be empty");
            }

            var pattern = namingConfig.StandardGameFileFormat;

            roms = roms.OrderBy(e => e.PlatformNumber).ThenBy(e => e.FileNumber).ToList();

            var splitPatterns = pattern.Split(new char[] { '\\', '/' }, StringSplitOptions.RemoveEmptyEntries);
            var components = new List<string>();

            for (var i = 0; i < splitPatterns.Length; i++)
            {
                var splitPattern = splitPatterns[i];
                var tokenHandlers = new Dictionary<string, Func<TokenMatch, string>>(FileNameBuilderTokenEqualityComparer.Instance);
                var patternHasRomIdentifier = GetPatternHasRomIdentifier(splitPattern);

                splitPattern = AddPlatformRomNumberingTokens(splitPattern, tokenHandlers, roms, namingConfig);
                splitPattern = AddAbsoluteNumberingTokens(splitPattern, tokenHandlers, game, roms, namingConfig);
                splitPattern = splitPattern.Replace("...", "{{ellipsis}}");

                UpdateMediaInfoIfNeeded(splitPattern, romFile, game);

                AddGameTokens(tokenHandlers, game);
                AddIdTokens(tokenHandlers, game);
                AddGameFileTokens(tokenHandlers, roms);
                AddRomTitlePlaceholderTokens(tokenHandlers);
                AddRomFileTokens(tokenHandlers, romFile, !patternHasRomIdentifier || romFile.Id == 0);
                AddQualityTokens(tokenHandlers, game, romFile);
                AddMediaInfoTokens(tokenHandlers, romFile);
                AddCustomFormats(tokenHandlers, game, romFile, customFormats);

                var component = ReplaceTokens(splitPattern, tokenHandlers, namingConfig, true).Trim();
                var maxPathSegmentLength = Math.Min(LongPathSupport.MaxFileNameLength, maxPath);
                if (i == splitPatterns.Length - 1)
                {
                    maxPathSegmentLength -= extension.GetByteCount();
                }

                var maxRomTitleLength = maxPathSegmentLength - GetLengthWithoutRomTitle(component, namingConfig);

                AddRomTitleTokens(tokenHandlers, roms, maxRomTitleLength);
                component = ReplaceTokens(component, tokenHandlers, namingConfig).Trim();

                component = FileNameCleanupRegex.Replace(component, match => match.Captures[0].Value[0].ToString());
                component = TrimSeparatorsRegex.Replace(component, string.Empty);
                component = component.Replace("{ellipsis}", "...");
                component = ReplaceReservedDeviceNames(component);

                components.Add(component);
            }

            return string.Join(Path.DirectorySeparatorChar.ToString(), components) + extension;
        }

        public string BuildFileName(List<Rom> roms, Game game, RomFile romFile, string extension = "", NamingConfig namingConfig = null, List<CustomFormat> customFormats = null)
        {
            return BuildFileName(roms, game, romFile, extension, LongPathSupport.MaxFilePathLength, namingConfig, customFormats);
        }

        public string BuildFilePath(List<Rom> roms, Game game, RomFile romFile, string extension, NamingConfig namingConfig = null, List<CustomFormat> customFormats = null)
        {
            Ensure.That(extension, () => extension).IsNotNullOrWhiteSpace();

            var platformPath = BuildPlatformPath(game, roms.First().PlatformNumber);
            var remainingPathLength = LongPathSupport.MaxFilePathLength - platformPath.GetByteCount() - 1;
            var fileName = BuildFileName(roms, game, romFile, extension, remainingPathLength, namingConfig, customFormats);

            return Path.Combine(platformPath, fileName);
        }

        public string BuildPlatformPath(Game game, int platformNumber)
        {
            var path = game.Path;

            if (game.PlatformFolder)
            {
                var platformFolder = GetPlatformFolder(game, platformNumber);

                platformFolder = CleanFileName(platformFolder);

                path = Path.Combine(path, platformFolder);
            }

            return path;
        }

        public string GetGameFolder(Game game, NamingConfig namingConfig = null)
        {
            if (namingConfig == null)
            {
                namingConfig = _namingConfigService.GetConfig();
            }

            var tokenHandlers = new Dictionary<string, Func<TokenMatch, string>>(FileNameBuilderTokenEqualityComparer.Instance);

            AddGameTokens(tokenHandlers, game);
            AddIdTokens(tokenHandlers, game);

            var folderName = ReplaceTokens(namingConfig.GameFolderFormat, tokenHandlers, namingConfig);

            folderName = CleanFolderName(folderName);
            folderName = ReplaceReservedDeviceNames(folderName);
            folderName = folderName.Replace("{ellipsis}", "...");

            return folderName;
        }

        public string GetPlatformFolder(Game game, int platformNumber, NamingConfig namingConfig = null)
        {
            if (namingConfig == null)
            {
                namingConfig = _namingConfigService.GetConfig();
            }

            var tokenHandlers = new Dictionary<string, Func<TokenMatch, string>>(FileNameBuilderTokenEqualityComparer.Instance);

            AddGameTokens(tokenHandlers, game);
            AddIdTokens(tokenHandlers, game);
            AddPlatformTokens(tokenHandlers, platformNumber);

            var format = namingConfig.PlatformFolderFormat;
            var folderName = ReplaceTokens(format, tokenHandlers, namingConfig);

            folderName = CleanFolderName(folderName);
            folderName = ReplaceReservedDeviceNames(folderName);
            folderName = folderName.Replace("{ellipsis}", "...");

            return folderName;
        }

        public static string CleanTitle(string title)
        {
            title = title.Replace("&", "and");
            title = ScenifyReplaceChars.Replace(title, " ");
            title = ScenifyRemoveChars.Replace(title, string.Empty);

            return title.RemoveDiacritics();
        }

        public static string TitleThe(string title)
        {
            return TitlePrefixRegex.Replace(title, "$2, $1$3");
        }

        public static string CleanTitleThe(string title)
        {
            if (TitlePrefixRegex.IsMatch(title))
            {
                var splitResult = TitlePrefixRegex.Split(title);
                return $"{CleanTitle(splitResult[2]).Trim()}, {splitResult[1]}{CleanTitle(splitResult[3])}";
            }

            return CleanTitle(title);
        }

        public static string TitleYear(string title, int year)
        {
            // Don't use 0 for the year.
            if (year == 0)
            {
                return title;
            }

            // Regex match in case the year in the title doesn't match the year, for whatever reason.
            if (YearRegex.IsMatch(title))
            {
                return title;
            }

            return $"{title} ({year})";
        }

        public static string CleanTitleTheYear(string title, int year)
        {
            // Don't use 0 for the year.
            if (year == 0)
            {
                return CleanTitleThe(title);
            }

            // Regex match in case the year in the title doesn't match the year, for whatever reason.
            if (YearRegex.IsMatch(title))
            {
                var splitReturn = YearRegex.Split(title);
                var yearMatch = YearRegex.Match(title);
                return $"{CleanTitleThe(splitReturn[0].Trim())} {yearMatch.Value[1..5]}";
            }

            return $"{CleanTitleThe(title)} {year}";
        }

        public static string TitleWithoutYear(string title)
        {
            title = YearRegex.Replace(title, "");

            return title;
        }

        public static string TitleFirstCharacter(string title)
        {
            if (char.IsLetterOrDigit(title[0]))
            {
                return title.Substring(0, 1).ToUpper().RemoveDiacritics()[0].ToString();
            }

            // Try the second character if the first was non alphanumeric
            if (char.IsLetterOrDigit(title[1]))
            {
                return title.Substring(1, 1).ToUpper().RemoveDiacritics()[0].ToString();
            }

            // Default to "_" if no alphanumeric character can be found in the first 2 positions
            return "_";
        }

        public static string CleanFileName(string name)
        {
            return CleanFileName(name, NamingConfig.Default);
        }

        public static string CleanFolderName(string name)
        {
            name = FileNameCleanupRegex.Replace(name, match => match.Captures[0].Value[0].ToString());

            return name.Trim(' ', '.');
        }

        public bool RequiresRomTitle(Game game, List<Rom> roms)
        {
            var namingConfig = _namingConfigService.GetConfig();
            var pattern = namingConfig.StandardGameFileFormat;

            if (!namingConfig.RenameGameFiles)
            {
                return false;
            }

            return _requiresRomTitleCache.Get(pattern, () =>
            {
                var matches = TitleRegex.Matches(pattern);

                foreach (Match match in matches)
                {
                    var token = match.Groups["token"].Value;

                    if (FileNameBuilderTokenEqualityComparer.Instance.Equals(token, "{Rom Title}") ||
                        FileNameBuilderTokenEqualityComparer.Instance.Equals(token, "{Rom CleanTitle}"))
                    {
                        return true;
                    }
                }

                return false;
            });
        }

        public bool RequiresAbsoluteRomNumber()
        {
            var namingConfig = _namingConfigService.GetConfig();
            var pattern = namingConfig.StandardGameFileFormat;

            return _requiresAbsoluteRomNumberCache.Get(pattern, () =>
            {
                var matches = AbsoluteGameFileRegex.Matches(pattern);

                return matches.Count > 0;
            });
        }

        private void AddGameTokens(Dictionary<string, Func<TokenMatch, string>> tokenHandlers, Game game)
        {
            tokenHandlers["{Game Title}"] = m => Truncate(game.Title, m.CustomFormat);
            tokenHandlers["{Game CleanTitle}"] = m => Truncate(CleanTitle(game.Title), m.CustomFormat);
            tokenHandlers["{Game TitleYear}"] = m => Truncate(TitleYear(game.Title, game.Year), m.CustomFormat);
            tokenHandlers["{Game CleanTitleYear}"] = m => Truncate(CleanTitle(TitleYear(game.Title, game.Year)), m.CustomFormat);
            tokenHandlers["{Game TitleWithoutYear}"] = m => Truncate(TitleWithoutYear(game.Title), m.CustomFormat);
            tokenHandlers["{Game CleanTitleWithoutYear}"] = m => Truncate(CleanTitle(TitleWithoutYear(game.Title)), m.CustomFormat);
            tokenHandlers["{Game TitleThe}"] = m => Truncate(TitleThe(game.Title), m.CustomFormat);
            tokenHandlers["{Game CleanTitleThe}"] = m => Truncate(CleanTitleThe(game.Title), m.CustomFormat);
            tokenHandlers["{Game TitleTheYear}"] = m => Truncate(TitleYear(TitleThe(game.Title), game.Year), m.CustomFormat);
            tokenHandlers["{Game CleanTitleTheYear}"] = m => Truncate(CleanTitleTheYear(game.Title, game.Year), m.CustomFormat);
            tokenHandlers["{Game TitleTheWithoutYear}"] = m => Truncate(TitleWithoutYear(TitleThe(game.Title)), m.CustomFormat);
            tokenHandlers["{Game CleanTitleTheWithoutYear}"] = m => Truncate(CleanTitleThe(TitleWithoutYear(game.Title)), m.CustomFormat);
            tokenHandlers["{Game TitleFirstCharacter}"] = m => Truncate(TitleFirstCharacter(TitleThe(game.Title)), m.CustomFormat);
            tokenHandlers["{Game Year}"] = m => game.Year.ToString();
        }

        private string AddPlatformRomNumberingTokens(string pattern, Dictionary<string, Func<TokenMatch, string>> tokenHandlers, List<Rom> roms, NamingConfig namingConfig)
        {
            var gameFileFormats = GetGameFileFormat(pattern).DistinctBy(v => v.PlatformGameFilePattern).ToList();

            var index = 1;
            foreach (var gameFileFormat in gameFileFormats)
            {
                var platformGameFilePattern = gameFileFormat.PlatformGameFilePattern;
                string formatPattern;

                switch ((MultiGameFileStyle)namingConfig.MultiGameFileStyle)
                {
                    case MultiGameFileStyle.Duplicate:
                        formatPattern = gameFileFormat.Separator + gameFileFormat.PlatformGameFilePattern;
                        platformGameFilePattern = FormatNumberTokens(platformGameFilePattern, formatPattern, roms);
                        break;

                    case MultiGameFileStyle.Repeat:
                        formatPattern = gameFileFormat.GameFileSeparator + gameFileFormat.GameFilePattern;
                        platformGameFilePattern = FormatNumberTokens(platformGameFilePattern, formatPattern, roms);
                        break;

                    case MultiGameFileStyle.Scene:
                        formatPattern = "-" + gameFileFormat.GameFileSeparator + gameFileFormat.GameFilePattern;
                        platformGameFilePattern = FormatNumberTokens(platformGameFilePattern, formatPattern, roms);
                        break;

                    case MultiGameFileStyle.Range:
                        formatPattern = "-" + gameFileFormat.GameFilePattern;
                        platformGameFilePattern = FormatRangeNumberTokens(platformGameFilePattern, formatPattern, roms);
                        break;

                    case MultiGameFileStyle.PrefixedRange:
                        formatPattern = "-" + gameFileFormat.GameFileSeparator + gameFileFormat.GameFilePattern;
                        platformGameFilePattern = FormatRangeNumberTokens(platformGameFilePattern, formatPattern, roms);
                        break;

                    // MultiGameFileStyle.Extend
                    default:
                        formatPattern = "-" + gameFileFormat.GameFilePattern;
                        platformGameFilePattern = FormatNumberTokens(platformGameFilePattern, formatPattern, roms);
                        break;
                }

                var token = string.Format("{{Platform Rom{0}}}", index++);
                pattern = pattern.Replace(gameFileFormat.PlatformGameFilePattern, token);
                tokenHandlers[token] = m => platformGameFilePattern;
            }

            AddPlatformTokens(tokenHandlers, roms.First().PlatformNumber);

            if (roms.Count > 1)
            {
                tokenHandlers["{Rom}"] = m => roms.First().FileNumber.ToString(m.CustomFormat) + "-" + roms.Last().FileNumber.ToString(m.CustomFormat);
            }
            else
            {
                tokenHandlers["{Rom}"] = m => roms.First().FileNumber.ToString(m.CustomFormat);
            }

            return pattern;
        }

        private string AddAbsoluteNumberingTokens(string pattern, Dictionary<string, Func<TokenMatch, string>> tokenHandlers, Game game, List<Rom> roms, NamingConfig namingConfig)
        {
            var absoluteGameFileFormats = GetAbsoluteFormat(pattern).DistinctBy(v => v.AbsoluteGameFilePattern).ToList();

            foreach (var absoluteGameFileFormat in absoluteGameFileFormats)
            {
                // Absolute numbering not used for games - strip tokens
                pattern = pattern.Replace(absoluteGameFileFormat.AbsoluteGameFilePattern, "");
            }

            return pattern;
        }

        private void AddPlatformTokens(Dictionary<string, Func<TokenMatch, string>> tokenHandlers, int platformNumber)
        {
            tokenHandlers["{Platform}"] = m => platformNumber.ToString(m.CustomFormat);
        }

        private void AddGameFileTokens(Dictionary<string, Func<TokenMatch, string>> tokenHandlers, List<Rom> roms)
        {
        }

        private void AddRomTitlePlaceholderTokens(Dictionary<string, Func<TokenMatch, string>> tokenHandlers)
        {
            tokenHandlers["{Rom Title}"] = m => null;
            tokenHandlers["{Rom CleanTitle}"] = m => null;
        }

        private void AddRomTitleTokens(Dictionary<string, Func<TokenMatch, string>> tokenHandlers, List<Rom> roms, int maxLength)
        {
            tokenHandlers["{Rom Title}"] = m => GetRomTitle(GetRomTitles(roms), "+", maxLength, m.CustomFormat);
            tokenHandlers["{Rom CleanTitle}"] = m => GetRomTitle(GetRomTitles(roms).Select(CleanTitle).ToList(), "and", maxLength, m.CustomFormat);
        }

        private void AddRomFileTokens(Dictionary<string, Func<TokenMatch, string>> tokenHandlers, RomFile romFile, bool useCurrentFilenameAsFallback)
        {
            tokenHandlers["{Original Title}"] = m => GetOriginalTitle(romFile, useCurrentFilenameAsFallback);
            tokenHandlers["{Original Filename}"] = m => GetOriginalFileName(romFile, useCurrentFilenameAsFallback);
            tokenHandlers["{Release Group}"] = m => romFile.ReleaseGroup.IsNullOrWhiteSpace() ? m.DefaultValue("Romarr") : Truncate(romFile.ReleaseGroup, m.CustomFormat);
            tokenHandlers["{Release Hash}"] = m => romFile.ReleaseHash ?? string.Empty;
        }

        private void AddQualityTokens(Dictionary<string, Func<TokenMatch, string>> tokenHandlers, Game game, RomFile romFile)
        {
            var qualityTitle = _qualityDefinitionService.Get(romFile.Quality.Quality).Title;
            var qualityProper = GetQualityProper(game, romFile.Quality);
            var qualityReal = GetQualityReal(game, romFile.Quality);

            tokenHandlers["{Quality Full}"] = m => string.Format("{0} {1} {2}", qualityTitle, qualityProper, qualityReal);
            tokenHandlers["{Quality Title}"] = m => qualityTitle;
            tokenHandlers["{Quality Proper}"] = m => qualityProper;
            tokenHandlers["{Quality Real}"] = m => qualityReal;
        }

        private static readonly IReadOnlyDictionary<string, int> MinimumMediaInfoSchemaRevisions =
            new Dictionary<string, int>(FileNameBuilderTokenEqualityComparer.Instance)
        {
            { MediaInfoVideoDynamicRangeToken, 5 },
            { MediaInfoVideoDynamicRangeTypeToken, 11 }
        };

        private void AddMediaInfoTokens(Dictionary<string, Func<TokenMatch, string>> tokenHandlers, RomFile romFile)
        {
            if (romFile.MediaInfo == null)
            {
                _logger.Trace("Media info is unavailable for {0}", romFile);

                return;
            }

            var sceneName = romFile.GetSceneOrFileName();

            var videoCodec = MediaInfoFormatter.FormatVideoCodec(romFile.MediaInfo, sceneName);
            var audioCodec = MediaInfoFormatter.FormatAudioCodec(romFile.MediaInfo.PrimaryAudioStream, sceneName);
            var audioChannels = MediaInfoFormatter.FormatAudioChannels(romFile.MediaInfo.PrimaryAudioStream);
            var audioLanguages = romFile.MediaInfo.AudioStreams?.Select(l => l.Language).ToList() ?? [];
            var subtitles = romFile.MediaInfo.SubtitleStreams?.Select(l => l.Language).ToList() ?? [];

            var videoBitDepth = romFile.MediaInfo.VideoBitDepth > 0 ? romFile.MediaInfo.VideoBitDepth.ToString() : 8.ToString();
            var audioChannelsFormatted = audioChannels > 0 ?
                                audioChannels.ToString("F1", CultureInfo.InvariantCulture) :
                                string.Empty;

            tokenHandlers["{MediaInfo File}"] = m => videoCodec;
            tokenHandlers["{MediaInfo FileCodec}"] = m => videoCodec;
            tokenHandlers["{MediaInfo FileBitDepth}"] = m => videoBitDepth;

            tokenHandlers["{MediaInfo Audio}"] = m => audioCodec;
            tokenHandlers["{MediaInfo AudioCodec}"] = m => audioCodec;
            tokenHandlers["{MediaInfo AudioChannels}"] = m => audioChannelsFormatted;
            tokenHandlers["{MediaInfo AudioLanguages}"] = m => GetLanguagesToken(audioLanguages, m.CustomFormat, true, true);
            tokenHandlers["{MediaInfo AudioLanguagesAll}"] = m => GetLanguagesToken(audioLanguages, m.CustomFormat, false, true);

            tokenHandlers["{MediaInfo SubtitleLanguages}"] = m => GetLanguagesToken(subtitles, m.CustomFormat, false, true);
            tokenHandlers["{MediaInfo SubtitleLanguagesAll}"] = m => GetLanguagesToken(subtitles, m.CustomFormat, false, true);

            tokenHandlers["{MediaInfo Simple}"] = m => $"{videoCodec} {audioCodec}";

            tokenHandlers["{MediaInfo Full}"] = m => $"{videoCodec} {audioCodec}{GetLanguagesToken(audioLanguages, m.CustomFormat, true, true)} {GetLanguagesToken(subtitles, m.CustomFormat, false, true)}";

            tokenHandlers[MediaInfoVideoDynamicRangeToken] =
                m => MediaInfoFormatter.FormatVideoDynamicRange(romFile.MediaInfo);
            tokenHandlers[MediaInfoVideoDynamicRangeTypeToken] =
                m => MediaInfoFormatter.FormatVideoDynamicRangeType(romFile.MediaInfo);
        }

        private void AddCustomFormats(Dictionary<string, Func<TokenMatch, string>> tokenHandlers, Game game, RomFile romFile, List<CustomFormat> customFormats = null)
        {
            if (customFormats == null)
            {
                romFile.Game = game;
                customFormats = _formatCalculator.ParseCustomFormat(romFile, game);
            }

            tokenHandlers["{Custom Formats}"] = m => GetCustomFormatsToken(customFormats, m.CustomFormat);
            tokenHandlers["{Custom Format}"] = m =>
            {
                if (m.CustomFormat.IsNullOrWhiteSpace())
                {
                    return string.Empty;
                }

                return customFormats.FirstOrDefault(x => x.IncludeCustomFormatWhenRenaming && x.Name == m.CustomFormat)?.ToString() ?? string.Empty;
            };
        }

        private void AddIdTokens(Dictionary<string, Func<TokenMatch, string>> tokenHandlers, Game game)
        {
            tokenHandlers["{ImdbId}"] = m => game.ImdbId ?? string.Empty;
            tokenHandlers["{IgdbId}"] = m => game.IgdbId.ToString();
            tokenHandlers["{RawgId}"] = m => game.RawgId > 0 ? game.RawgId.ToString() : string.Empty;
            tokenHandlers["{TmdbId}"] = m => game.TmdbId > 0 ? game.TmdbId.ToString() : string.Empty;
        }

        private string GetCustomFormatsToken(List<CustomFormat> customFormats, string filter)
        {
            var tokens = customFormats.Where(x => x.IncludeCustomFormatWhenRenaming).ToList();

            var filteredTokens = tokens;

            if (filter.IsNotNullOrWhiteSpace())
            {
                if (filter.StartsWith("-"))
                {
                    var splitFilter = filter.Substring(1).Split(',');
                    filteredTokens = tokens.Where(c => !splitFilter.Contains(c.Name)).ToList();
                }
                else
                {
                    var splitFilter = filter.Split(',');
                    filteredTokens = tokens.Where(c => splitFilter.Contains(c.Name)).ToList();
                }
            }

            return string.Join(" ", filteredTokens);
        }

        private string GetLanguagesToken(List<string> mediaInfoLanguages, string filter, bool skipEnglishOnly, bool quoted)
        {
            var tokens = new List<string>();
            foreach (var item in mediaInfoLanguages)
            {
                if (!string.IsNullOrWhiteSpace(item) && item != "und")
                {
                    tokens.Add(item.Trim());
                }
            }

            for (var i = 0; i < tokens.Count; i++)
            {
                try
                {
                    var token = tokens[i].ToLowerInvariant();
                    if (Iso639BTMap.TryGetValue(token, out var mapped))
                    {
                        token = mapped;
                    }

                    var cultureInfo = new CultureInfo(token);
                    tokens[i] = cultureInfo.TwoLetterISOLanguageName.ToUpper();
                }
                catch
                {
                }
            }

            tokens = tokens.Distinct().ToList();

            var filteredTokens = tokens;

            // Exclude or filter
            if (filter.IsNotNullOrWhiteSpace())
            {
                if (filter.StartsWith("-"))
                {
                    filteredTokens = tokens.Except(filter.Split('-')).ToList();
                }
                else
                {
                    filteredTokens = filter.Split('+').Intersect(tokens).ToList();
                }
            }

            // Replace with wildcard (maybe too limited)
            if (filter.IsNotNullOrWhiteSpace() && filter.EndsWith("+") && filteredTokens.Count != tokens.Count)
            {
                filteredTokens.Add("--");
            }

            if (skipEnglishOnly && filteredTokens.Count == 1 && filteredTokens.First() == "EN")
            {
                return string.Empty;
            }

            var response = string.Join("+", filteredTokens);

            if (quoted && response.IsNotNullOrWhiteSpace())
            {
                return $"[{response}]";
            }
            else
            {
                return response;
            }
        }

        private void UpdateMediaInfoIfNeeded(string pattern, RomFile romFile, Game game)
        {
            if (game.Path.IsNullOrWhiteSpace())
            {
                return;
            }

            var schemaRevision = romFile.MediaInfo != null ? romFile.MediaInfo.SchemaRevision : 0;
            var matches = TitleRegex.Matches(pattern);

            var shouldUpdateMediaInfo = matches.Cast<Match>()
                .Select(m => MinimumMediaInfoSchemaRevisions.GetValueOrDefault(m.Value, -1))
                .Any(r => schemaRevision < r);

            if (shouldUpdateMediaInfo)
            {
                _mediaInfoUpdater.Update(romFile, game);
            }
        }

        private string ReplaceTokens(string pattern, Dictionary<string, Func<TokenMatch, string>> tokenHandlers, NamingConfig namingConfig, bool escape = false)
        {
            return TitleRegex.Replace(pattern, match => ReplaceToken(match, tokenHandlers, namingConfig, escape));
        }

        private string ReplaceToken(Match match, Dictionary<string, Func<TokenMatch, string>> tokenHandlers, NamingConfig namingConfig, bool escape)
        {
            if (match.Groups["escaped"].Success)
            {
                if (escape)
                {
                    return match.Value;
                }
                else if (match.Value == "{{")
                {
                    return "{";
                }
                else if (match.Value == "}}")
                {
                    return "}";
                }
            }

            var tokenMatch = new TokenMatch
            {
                RegexMatch = match,
                Prefix = match.Groups["prefix"].Value,
                Separator = match.Groups["separator"].Value,
                Suffix = match.Groups["suffix"].Value,
                Token = match.Groups["token"].Value,
                CustomFormat = match.Groups["customFormat"].Value
            };

            if (tokenMatch.CustomFormat.IsNullOrWhiteSpace())
            {
                tokenMatch.CustomFormat = null;
            }

            var tokenHandler = tokenHandlers.GetValueOrDefault(tokenMatch.Token, m => string.Empty);

            var replacementText = tokenHandler(tokenMatch);

            if (replacementText == null)
            {
                // Preserve original token if handler returned null
                return match.Value;
            }

            replacementText = replacementText.Trim();

            if (tokenMatch.Token.All(t => !char.IsLetter(t) || char.IsLower(t)))
            {
                replacementText = replacementText.ToLower();
            }
            else if (tokenMatch.Token.All(t => !char.IsLetter(t) || char.IsUpper(t)))
            {
                replacementText = replacementText.ToUpper();
            }

            if (!tokenMatch.Separator.IsNullOrWhiteSpace())
            {
                replacementText = replacementText.Replace(" ", tokenMatch.Separator);
            }

            replacementText = CleanFileName(replacementText, namingConfig);

            if (!replacementText.IsNullOrWhiteSpace())
            {
                replacementText = tokenMatch.Prefix + replacementText + tokenMatch.Suffix;
            }

            if (escape)
            {
                replacementText = replacementText.Replace("{", "{{").Replace("}", "}}");
            }

            return replacementText;
        }

        private string FormatNumberTokens(string basePattern, string formatPattern, List<Rom> roms)
        {
            var pattern = string.Empty;

            for (var i = 0; i < roms.Count; i++)
            {
                var patternToReplace = i == 0 ? basePattern : formatPattern;

                pattern += GameFileRegex.Replace(patternToReplace, match => ReplaceNumberToken(match.Groups["rom"].Value, roms[i].FileNumber));
            }

            return ReplacePlatformTokens(pattern, roms.First().PlatformNumber);
        }

        private string FormatAbsoluteNumberTokens(string basePattern, string formatPattern, List<Rom> roms)
        {
            var pattern = string.Empty;

            for (var i = 0; i < roms.Count; i++)
            {
                var patternToReplace = i == 0 ? basePattern : formatPattern;

                pattern += AbsoluteGameFileRegex.Replace(patternToReplace, match => ReplaceNumberToken(match.Groups["absolute"].Value, roms[i].AbsoluteFileNumber.Value));
            }

            return ReplacePlatformTokens(pattern, roms.First().PlatformNumber);
        }

        private string FormatRangeNumberTokens(string platformGameFilePattern, string formatPattern, List<Rom> roms)
        {
            var eps = new List<Rom> { roms.First() };

            if (roms.Count > 1)
            {
                eps.Add(roms.Last());
            }

            return FormatNumberTokens(platformGameFilePattern, formatPattern, eps);
        }

        private string ReplacePlatformTokens(string pattern, int platformNumber)
        {
            return PlatformRegex.Replace(pattern, match => ReplaceNumberToken(match.Groups["platform"].Value, platformNumber));
        }

        private string ReplaceNumberToken(string token, int value)
        {
            var split = token.Trim('{', '}').Split(':');
            if (split.Length == 1)
            {
                return value.ToString("0");
            }

            return value.ToString(split[1]);
        }

        private GameFileFormat[] GetGameFileFormat(string pattern)
        {
            return _gameFileFormatCache.Get(pattern, () => PlatformGameFilePatternRegex.Matches(pattern).OfType<Match>()
                .Select(match => new GameFileFormat
                {
                    GameFileSeparator = match.Groups["gameFileSeparator"].Value,
                    Separator = match.Groups["separator"].Value,
                    GameFilePattern = match.Groups["rom"].Value,
                    PlatformGameFilePattern = match.Groups["platformGameFile"].Value,
                }).ToArray());
        }

        private AbsoluteGameFileFormat[] GetAbsoluteFormat(string pattern)
        {
            return _absoluteGameFileFormatCache.Get(pattern, () => AbsoluteGameFilePatternRegex.Matches(pattern).OfType<Match>()
                .Select(match => new AbsoluteGameFileFormat
                {
                    Separator = match.Groups["separator"].Value.IsNotNullOrWhiteSpace() ? match.Groups["separator"].Value : "-",
                    AbsoluteGameFilePattern = match.Groups["absolute"].Value
                }).ToArray());
        }

        private bool GetPatternHasRomIdentifier(string pattern)
        {
            return _patternHasRomIdentifierCache.Get(pattern, () =>
            {
                if (PlatformGameFilePatternRegex.IsMatch(pattern))
                {
                    return true;
                }

                if (AbsoluteGameFilePatternRegex.IsMatch(pattern))
                {
                    return true;
                }

                return false;
            });
        }

        private List<string> GetRomTitles(List<Rom> roms)
        {
            if (roms.Count == 1)
            {
                return new List<string>
                       {
                           roms.First().Title.TrimEnd(RomTitleTrimCharacters)
                       };
            }

            var titles = roms.Select(c => c.Title.TrimEnd(RomTitleTrimCharacters))
                                 .Select(CleanupRomTitle)
                                 .Distinct()
                                 .ToList();

            if (titles.All(t => t.IsNullOrWhiteSpace()))
            {
                titles = roms.Select(c => c.Title.TrimEnd(RomTitleTrimCharacters))
                                 .Distinct()
                                 .ToList();
            }

            return titles;
        }

        private string GetRomTitle(List<string> titles, string separator, int maxLength, string formatter)
        {
            var maxFormatterLength = GetMaxLengthFromFormatter(formatter);

            if (maxFormatterLength > 0)
            {
                maxLength = Math.Min(maxLength, maxFormatterLength);
            }

            separator = $" {separator.Trim()} ";

            var joined = string.Join(separator, titles);

            if (joined.GetByteCount() <= maxLength)
            {
                return joined;
            }

            var firstTitle = titles.First();
            var firstTitleLength = firstTitle.GetByteCount();

            if (titles.Count >= 2)
            {
                var lastTitle = titles.Last();
                var lastTitleLength = lastTitle.GetByteCount();
                if (firstTitleLength + lastTitleLength + 3 <= maxLength)
                {
                    return $"{firstTitle.TrimEnd(' ', '.')}{{ellipsis}}{lastTitle}";
                }
            }

            if (titles.Count > 1 && firstTitleLength + 3 <= maxLength)
            {
                return $"{firstTitle.TrimEnd(' ', '.')}{{ellipsis}}";
            }

            if (titles.Count == 1 && firstTitleLength <= maxLength)
            {
                return firstTitle;
            }

            return $"{firstTitle.Truncate(maxLength - 3).TrimEnd(' ', '.')}{{ellipsis}}";
        }

        private string CleanupRomTitle(string title)
        {
            // this will remove (1),(2) from the end of multi part roms.
            return MultiPartCleanupRegex.Replace(title, string.Empty).Trim();
        }

        private string GetQualityProper(Game game, QualityModel quality)
        {
            if (quality.Revision.Version > 1)
            {
                return "Proper";
            }

            return string.Empty;
        }

        private string GetQualityReal(Game game, QualityModel quality)
        {
            if (quality.Revision.Real > 0)
            {
                return "REAL";
            }

            return string.Empty;
        }

        private string GetOriginalTitle(RomFile romFile, bool useCurrentFilenameAsFallback)
        {
            if (romFile.SceneName.IsNullOrWhiteSpace())
            {
                return CleanFileName(GetOriginalFileName(romFile, useCurrentFilenameAsFallback));
            }

            return CleanFileName(romFile.SceneName);
        }

        private string GetOriginalFileName(RomFile romFile, bool useCurrentFilenameAsFallback)
        {
            if (!useCurrentFilenameAsFallback)
            {
                return string.Empty;
            }

            if (romFile.RelativePath.IsNullOrWhiteSpace())
            {
                return Path.GetFileNameWithoutExtension(romFile.Path);
            }

            return Path.GetFileNameWithoutExtension(romFile.RelativePath);
        }

        private int GetLengthWithoutRomTitle(string pattern, NamingConfig namingConfig)
        {
            var tokenHandlers = new Dictionary<string, Func<TokenMatch, string>>(FileNameBuilderTokenEqualityComparer.Instance);
            tokenHandlers["{Rom Title}"] = m => string.Empty;
            tokenHandlers["{Rom CleanTitle}"] = m => string.Empty;
            tokenHandlers["{ellipsis}"] = m => "...";

            var result = ReplaceTokens(pattern, tokenHandlers, namingConfig);

            return result.GetByteCount();
        }

        private string ReplaceReservedDeviceNames(string input)
        {
            // Replace reserved windows device names with an alternative
            return ReservedDeviceNamesRegex.Replace(input, match => match.Value.Replace(".", "_"));
        }

        private static string CleanFileName(string name, NamingConfig namingConfig)
        {
            var result = name;

            if (namingConfig.ReplaceIllegalCharacters)
            {
                // Smart replaces a colon followed by a space with space dash space for a better appearance
                if (namingConfig.ColonReplacementFormat == ColonReplacementFormat.Smart)
                {
                    result = result.Replace(": ", " - ");
                    result = result.Replace(":", "-");
                }
                else
                {
                    var replacement = string.Empty;

                    switch (namingConfig.ColonReplacementFormat)
                    {
                        case ColonReplacementFormat.Dash:
                            replacement = "-";
                            break;
                        case ColonReplacementFormat.SpaceDash:
                            replacement = " -";
                            break;
                        case ColonReplacementFormat.SpaceDashSpace:
                            replacement = " - ";
                            break;
                        case ColonReplacementFormat.Custom:
                            replacement = namingConfig.CustomColonReplacementFormat;
                            break;
                    }

                    result = result.Replace(":", replacement);
                }
            }
            else
            {
                result = result.Replace(":", string.Empty);
            }

            for (var i = 0; i < BadCharacters.Length; i++)
            {
                result = result.Replace(BadCharacters[i], namingConfig.ReplaceIllegalCharacters ? GoodCharacters[i] : string.Empty);
            }

            return result.TrimStart(' ', '.').TrimEnd(' ');
        }

        private string Truncate(string input, string formatter)
        {
            if (input.IsNullOrWhiteSpace())
            {
                return string.Empty;
            }

            var maxLength = GetMaxLengthFromFormatter(formatter);

            if (maxLength == 0 || input.Length <= Math.Abs(maxLength))
            {
                return input;
            }

            if (maxLength < 0)
            {
                return $"{{ellipsis}}{input.Reverse().Truncate(Math.Abs(maxLength) - 3).TrimEnd(' ', '.').Reverse()}";
            }

            return $"{input.Truncate(maxLength - 3).TrimEnd(' ', '.')}{{ellipsis}}";
        }

        private int GetMaxLengthFromFormatter(string formatter)
        {
            int.TryParse(formatter, out var maxCustomLength);

            return maxCustomLength;
        }
    }

    internal sealed class TokenMatch
    {
        public Match RegexMatch { get; set; }
        public string Prefix { get; set; }
        public string Separator { get; set; }
        public string Suffix { get; set; }
        public string Token { get; set; }
        public string CustomFormat { get; set; }

        public string DefaultValue(string defaultValue)
        {
            if (string.IsNullOrEmpty(Prefix) && string.IsNullOrEmpty(Suffix))
            {
                return defaultValue;
            }
            else
            {
                return string.Empty;
            }
        }
    }

    public enum MultiGameFileStyle
    {
        Extend = 0,
        Duplicate = 1,
        Repeat = 2,
        Scene = 3,
        Range = 4,
        PrefixedRange = 5
    }

    public enum ColonReplacementFormat
    {
        Delete = 0,
        Dash = 1,
        SpaceDash = 2,
        SpaceDashSpace = 3,
        Smart = 4,
        Custom = 5
    }
}
