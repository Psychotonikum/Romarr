using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NLog;
using Romarr.Common.Disk;
using Romarr.Common.Extensions;
using Romarr.Common.Processes;
using Romarr.Core.Configuration;
using Romarr.Core.MediaFiles.MediaInfo;
using Romarr.Core.Parser;
using Romarr.Core.Parser.Model;
using Romarr.Core.Tags;

namespace Romarr.Core.MediaFiles
{
    public interface IImportScript
    {
        public ScriptImportDecision TryImport(string sourcePath, string destinationFilePath, LocalGameFile localRom, RomFile romFile, TransferMode mode);
    }

    public class ImportScriptService : IImportScript
    {
        private readonly IConfigFileProvider _configFileProvider;
        private readonly IGameFileInfoReader _videoFileInfoReader;
        private readonly IProcessProvider _processProvider;
        private readonly IConfigService _configService;
        private readonly ITagRepository _tagRepository;
        private readonly IDiskProvider _diskProvider;
        private readonly Logger _logger;

        public ImportScriptService(IProcessProvider processProvider,
                                   IGameFileInfoReader videoFileInfoReader,
                                   IConfigService configService,
                                   IConfigFileProvider configFileProvider,
                                   ITagRepository tagRepository,
                                   IDiskProvider diskProvider,
                                   Logger logger)
        {
            _processProvider = processProvider;
            _videoFileInfoReader = videoFileInfoReader;
            _configService = configService;
            _configFileProvider = configFileProvider;
            _tagRepository = tagRepository;
            _diskProvider = diskProvider;
            _logger = logger;
        }

        private static readonly Regex OutputRegex = new Regex(@"^(?:\[(?:(?<mediaFile>MediaFile)|(?<extraFile>ExtraFile))\]\s?(?<fileName>.+)|(?<preventExtraImport>\[PreventExtraImport\])|\[MoveStatus\]\s?(?:(?<deferMove>DeferMove)|(?<moveComplete>MoveComplete)|(?<renameRequested>RenameRequested)))$", RegexOptions.Compiled);

        private ScriptImportInfo ProcessOutput(List<ProcessOutputLine> processOutputLines)
        {
            var possibleExtraFiles = new List<string>();
            string mediaFile = null;
            var decision = ScriptImportDecision.MoveComplete;
            var importExtraFiles = true;

            foreach (var line in processOutputLines)
            {
                var match = OutputRegex.Match(line.Content);

                if (match.Groups["mediaFile"].Success)
                {
                    if (mediaFile is not null)
                    {
                        throw new ScriptImportException("Script output contains multiple media files. Only one media file can be returned.");
                    }

                    mediaFile = match.Groups["fileName"].Value;

                    if (!MediaFileExtensions.Extensions.Contains(Path.GetExtension(mediaFile)))
                    {
                        throw new ScriptImportException("Script output contains invalid media file: {0}", mediaFile);
                    }
                    else if (!_diskProvider.FileExists(mediaFile))
                    {
                        throw new ScriptImportException("Script output contains non-existent media file: {0}", mediaFile);
                    }
                }
                else if (match.Groups["extraFile"].Success)
                {
                    var fileName = match.Groups["fileName"].Value;

                    if (!_diskProvider.FileExists(fileName))
                    {
                        _logger.Warn("Script output contains non-existent possible extra file: {0}", fileName);
                    }

                    possibleExtraFiles.Add(fileName);
                }
                else if (match.Groups["moveComplete"].Success)
                {
                    decision = ScriptImportDecision.MoveComplete;
                }
                else if (match.Groups["renameRequested"].Success)
                {
                    decision = ScriptImportDecision.RenameRequested;
                }
                else if (match.Groups["deferMove"].Success)
                {
                    decision = ScriptImportDecision.DeferMove;
                }
                else if (match.Groups["preventExtraImport"].Success)
                {
                    importExtraFiles = false;
                }
            }

            return new ScriptImportInfo(possibleExtraFiles, mediaFile, decision, importExtraFiles);
        }

        public ScriptImportDecision TryImport(string sourcePath, string destinationFilePath, LocalGameFile localRom, RomFile romFile, TransferMode mode)
        {
            var game = localRom.Game;
            var oldFiles = localRom.OldFiles;
            var downloadClientInfo = localRom.DownloadItem?.DownloadClientInfo;
            var downloadId = localRom.DownloadItem?.DownloadId;

            if (!_configService.UseScriptImport)
            {
                return ScriptImportDecision.DeferMove;
            }

            var environmentVariables = new StringDictionary();

            environmentVariables.Add("Romarr_SourcePath", sourcePath);
            environmentVariables.Add("Romarr_DestinationPath", destinationFilePath);

            environmentVariables.Add("Romarr_InstanceName", _configFileProvider.InstanceName);
            environmentVariables.Add("Romarr_ApplicationUrl", _configService.ApplicationUrl);
            environmentVariables.Add("Romarr_TransferMode", mode.ToString());

            environmentVariables.Add("Romarr_Series_Id", game.Id.ToString());
            environmentVariables.Add("Romarr_Series_Title", game.Title);
            environmentVariables.Add("Romarr_Series_TitleSlug", game.TitleSlug);
            environmentVariables.Add("Romarr_Series_Path", game.Path);
            environmentVariables.Add("Romarr_Series_IgdbId", game.IgdbId.ToString());
            environmentVariables.Add("Romarr_Series_RawgId", game.RawgId.ToString());
            environmentVariables.Add("Romarr_Series_TmdbId", game.TmdbId.ToString());
            environmentVariables.Add("Romarr_Series_ImdbId", game.ImdbId ?? string.Empty);
            environmentVariables.Add("Romarr_Series_Type", game.GameType.ToString());
            environmentVariables.Add("Romarr_Series_OriginalLanguage", IsoLanguages.Get(game.OriginalLanguage).ThreeLetterCode);
            environmentVariables.Add("Romarr_Series_Genres", string.Join("|", game.Genres));
            environmentVariables.Add("Romarr_Series_Tags", string.Join("|", game.Tags.Select(t => _tagRepository.Get(t).Label)));

            environmentVariables.Add("Romarr_RomFile_GameFileCount", localRom.Roms.Count.ToString());
            environmentVariables.Add("Romarr_RomFile_RomIds", string.Join(",", localRom.Roms.Select(e => e.Id)));
            environmentVariables.Add("Romarr_RomFile_PlatformNumber", localRom.PlatformNumber.ToString());
            environmentVariables.Add("Romarr_RomFile_RomNumbers", string.Join(",", localRom.Roms.Select(e => e.FileNumber)));
            environmentVariables.Add("Romarr_RomFile_GameFileAirDates", string.Join(",", localRom.Roms.Select(e => e.AirDate)));
            environmentVariables.Add("Romarr_RomFile_GameFileAirDatesUtc", string.Join(",", localRom.Roms.Select(e => e.AirDateUtc)));
            environmentVariables.Add("Romarr_RomFile_RomTitles", string.Join("|", localRom.Roms.Select(e => e.Title)));
            environmentVariables.Add("Romarr_RomFile_GameFileOverviews", string.Join("|", localRom.Roms.Select(e => e.Overview)));
            environmentVariables.Add("Romarr_RomFile_Quality", localRom.Quality.Quality.Name);
            environmentVariables.Add("Romarr_RomFile_QualityVersion", localRom.Quality.Revision.Version.ToString());
            environmentVariables.Add("Romarr_RomFile_ReleaseGroup", localRom.ReleaseGroup ?? string.Empty);
            environmentVariables.Add("Romarr_RomFile_SceneName", localRom.SceneName ?? string.Empty);

            environmentVariables.Add("Romarr_Download_Client", downloadClientInfo?.Name ?? string.Empty);
            environmentVariables.Add("Romarr_Download_Client_Type", downloadClientInfo?.Type ?? string.Empty);
            environmentVariables.Add("Romarr_Download_Id", downloadId ?? string.Empty);

            if (localRom.MediaInfo == null)
            {
                _logger.Trace("MediaInfo is null for rom file import. This may cause issues with the import script.");
            }
            else
            {
                environmentVariables.Add("Romarr_RomFile_MediaInfo_AudioChannels",
                    MediaInfoFormatter.FormatAudioChannels(localRom.MediaInfo.PrimaryAudioStream).ToString());
                environmentVariables.Add("Romarr_RomFile_MediaInfo_AudioCodec",
                    MediaInfoFormatter.FormatAudioCodec(localRom.MediaInfo.PrimaryAudioStream, null));
                environmentVariables.Add("Romarr_RomFile_MediaInfo_AudioLanguages",
                    localRom.MediaInfo.AudioStreams?.Select(l => l.Language).Distinct().ConcatToString(" / "));
                environmentVariables.Add("Romarr_RomFile_MediaInfo_Languages",
                    localRom.MediaInfo.AudioStreams?.Select(l => l.Language).ConcatToString(" / "));
                environmentVariables.Add("Romarr_RomFile_MediaInfo_Height",
                    localRom.MediaInfo.Height.ToString());
                environmentVariables.Add("Romarr_RomFile_MediaInfo_Width", localRom.MediaInfo.Width.ToString());
                environmentVariables.Add("Romarr_RomFile_MediaInfo_Subtitles",
                    localRom.MediaInfo.SubtitleStreams?.Select(l => l.Language).ConcatToString(" / "));
                environmentVariables.Add("Romarr_RomFile_MediaInfo_VideoCodec",
                    MediaInfoFormatter.FormatVideoCodec(localRom.MediaInfo, null));
                environmentVariables.Add("Romarr_RomFile_MediaInfo_VideoDynamicRangeType",
                    MediaInfoFormatter.FormatVideoDynamicRangeType(localRom.MediaInfo));
            }

            environmentVariables.Add("Romarr_RomFile_CustomFormat", string.Join("|", localRom.CustomFormats));
            environmentVariables.Add("Romarr_RomFile_CustomFormatScore", localRom.CustomFormatScore.ToString());

            if (oldFiles.Any())
            {
                environmentVariables.Add("Romarr_DeletedRelativePaths", string.Join("|", oldFiles.Select(e => e.RomFile.RelativePath)));
                environmentVariables.Add("Romarr_DeletedPaths", string.Join("|", oldFiles.Select(e => Path.Combine(game.Path, e.RomFile.RelativePath))));
                environmentVariables.Add("Romarr_DeletedDateAdded", string.Join("|", oldFiles.Select(e => e.RomFile.DateAdded)));
                environmentVariables.Add("Romarr_DeletedRecycleBinPaths", string.Join("|", oldFiles.Select(e => e.RecycleBinPath ?? string.Empty)));
            }

            _logger.Debug("Executing external script: {0}", _configService.ScriptImportPath);

            var processOutput = _processProvider.StartAndCapture(_configService.ScriptImportPath, $"\"{sourcePath}\" \"{destinationFilePath}\"", environmentVariables);

            _logger.Debug("Script Output: \r\n{0}", string.Join("\r\n", processOutput.Lines));

            if (processOutput.ExitCode != 0)
            {
                throw new ScriptImportException("Script exited with non-zero exit code: {0}", processOutput.ExitCode);
            }

            var scriptImportInfo = ProcessOutput(processOutput.Lines);

            var mediaFile = scriptImportInfo.MediaFile ?? destinationFilePath;
            localRom.PossibleExtraFiles = scriptImportInfo.PossibleExtraFiles;

            romFile.RelativePath = game.Path.GetRelativePath(mediaFile);
            romFile.Path = mediaFile;

            var exitCode = processOutput.ExitCode;

            localRom.ShouldImportExtras = scriptImportInfo.ImportExtraFiles;

            if (scriptImportInfo.Decision != ScriptImportDecision.DeferMove)
            {
                localRom.ScriptImported = true;
            }

            if (scriptImportInfo.Decision == ScriptImportDecision.RenameRequested)
            {
                romFile.MediaInfo = _videoFileInfoReader.GetMediaInfo(mediaFile);
                romFile.Path = null;
            }

            return scriptImportInfo.Decision;
        }
    }
}
