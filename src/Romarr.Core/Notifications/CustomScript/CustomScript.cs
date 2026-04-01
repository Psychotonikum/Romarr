using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using FluentValidation.Results;
using NLog;
using Romarr.Common.Disk;
using Romarr.Common.Extensions;
using Romarr.Common.Processes;
using Romarr.Core.Configuration;
using Romarr.Core.HealthCheck;
using Romarr.Core.Localization;
using Romarr.Core.MediaFiles;
using Romarr.Core.MediaFiles.MediaInfo;
using Romarr.Core.Parser;
using Romarr.Core.Tags;
using Romarr.Core.ThingiProvider;
using Romarr.Core.Games;
using Romarr.Core.Validation;

namespace Romarr.Core.Notifications.CustomScript
{
    public class CustomScript : NotificationBase<CustomScriptSettings>
    {
        private readonly IConfigFileProvider _configFileProvider;
        private readonly IConfigService _configService;
        private readonly IDiskProvider _diskProvider;
        private readonly IProcessProvider _processProvider;
        private readonly ITagRepository _tagRepository;
        private readonly ILocalizationService _localizationService;
        private readonly Logger _logger;

        public CustomScript(IConfigFileProvider configFileProvider,
            IConfigService configService,
            IDiskProvider diskProvider,
            IProcessProvider processProvider,
            ITagRepository tagRepository,
            ILocalizationService localizationService,
            Logger logger)
        {
            _configFileProvider = configFileProvider;
            _configService = configService;
            _diskProvider = diskProvider;
            _processProvider = processProvider;
            _tagRepository = tagRepository;
            _localizationService = localizationService;
            _logger = logger;
        }

        public override string Name => _localizationService.GetLocalizedString("NotificationsCustomScriptSettingsName");

        public override string Link => "https://wiki.servarr.com/romarr/settings#connections";

        public override ProviderMessage Message => new ProviderMessage(_localizationService.GetLocalizedString("NotificationsCustomScriptSettingsProviderMessage", new Dictionary<string, object> { { "eventTypeTest", "Test" } }), ProviderMessageType.Warning);

        public override void OnGrab(GrabMessage message)
        {
            var game = message.Game;
            var remoteRom = message.Rom;
            var releaseGroup = remoteRom.ParsedRomInfo.ReleaseGroup;
            var environmentVariables = new StringDictionary();

            AddInstanceVariables(environmentVariables, "Grab");
            AddGameVariables(environmentVariables, game);

            environmentVariables.Add("Romarr_Release_GameFileCount", remoteRom.Roms.Count.ToString());
            environmentVariables.Add("Romarr_Release_PlatformNumber", remoteRom.Roms.First().PlatformNumber.ToString());
            environmentVariables.Add("Romarr_Release_RomNumbers", string.Join(",", remoteRom.Roms.Select(e => e.FileNumber)));
            environmentVariables.Add("Romarr_Release_AbsoluteRomNumbers", string.Join(",", remoteRom.Roms.Select(e => e.AbsoluteFileNumber)));
            environmentVariables.Add("Romarr_Release_GameFileAirDates", string.Join(",", remoteRom.Roms.Select(e => e.AirDate)));
            environmentVariables.Add("Romarr_Release_GameFileAirDatesUtc", string.Join(",", remoteRom.Roms.Select(e => e.AirDateUtc)));
            environmentVariables.Add("Romarr_Release_RomTitles", string.Join("|", remoteRom.Roms.Select(e => e.Title)));
            environmentVariables.Add("Romarr_Release_GameFileOverviews", string.Join("|", remoteRom.Roms.Select(e => e.Overview)));
            environmentVariables.Add("Romarr_Release_FinaleTypes", string.Join("|", remoteRom.Roms.Select(e => e.FinaleType)));
            environmentVariables.Add("Romarr_Release_Title", remoteRom.Release.Title);
            environmentVariables.Add("Romarr_Release_Indexer", remoteRom.Release.Indexer ?? string.Empty);
            environmentVariables.Add("Romarr_Release_Size", remoteRom.Release.Size.ToString());
            environmentVariables.Add("Romarr_Release_Quality", remoteRom.ParsedRomInfo.Quality.Quality.Name);
            environmentVariables.Add("Romarr_Release_QualityVersion", remoteRom.ParsedRomInfo.Quality.Revision.Version.ToString());
            environmentVariables.Add("Romarr_Release_ReleaseGroup", releaseGroup ?? string.Empty);
            environmentVariables.Add("Romarr_Release_IndexerFlags", remoteRom.Release.IndexerFlags.ToString());
            environmentVariables.Add("Romarr_Download_Client", message.DownloadClientName ?? string.Empty);
            environmentVariables.Add("Romarr_Download_Client_Type", message.DownloadClientType ?? string.Empty);
            environmentVariables.Add("Romarr_Download_Id", message.DownloadId ?? string.Empty);
            environmentVariables.Add("Romarr_Release_CustomFormat", string.Join("|", remoteRom.CustomFormats));
            environmentVariables.Add("Romarr_Release_CustomFormatScore", remoteRom.CustomFormatScore.ToString());
            environmentVariables.Add("Romarr_Release_ReleaseType", remoteRom.ParsedRomInfo.ReleaseType.ToString());

            ExecuteScript(environmentVariables);
        }

        public override void OnDownload(DownloadMessage message)
        {
            var game = message.Game;
            var romFile = message.RomFile;
            var sourcePath = message.SourcePath;
            var environmentVariables = new StringDictionary();

            AddInstanceVariables(environmentVariables, "Download");
            AddGameVariables(environmentVariables, game);

            environmentVariables.Add("Romarr_IsUpgrade", message.OldFiles.Any().ToString());
            environmentVariables.Add("Romarr_RomFile_Id", romFile.Id.ToString());
            environmentVariables.Add("Romarr_RomFile_GameFileCount", romFile.Roms.Value.Count.ToString());
            environmentVariables.Add("Romarr_RomFile_RelativePath", romFile.RelativePath);
            environmentVariables.Add("Romarr_RomFile_Path", Path.Combine(game.Path, romFile.RelativePath));
            environmentVariables.Add("Romarr_RomFile_RomIds", string.Join(",", romFile.Roms.Value.Select(e => e.Id)));
            environmentVariables.Add("Romarr_RomFile_PlatformNumber", romFile.PlatformNumber.ToString());
            environmentVariables.Add("Romarr_RomFile_RomNumbers", string.Join(",", romFile.Roms.Value.Select(e => e.FileNumber)));
            environmentVariables.Add("Romarr_RomFile_GameFileAirDates", string.Join(",", romFile.Roms.Value.Select(e => e.AirDate)));
            environmentVariables.Add("Romarr_RomFile_GameFileAirDatesUtc", string.Join(",", romFile.Roms.Value.Select(e => e.AirDateUtc)));
            environmentVariables.Add("Romarr_RomFile_RomTitles", string.Join("|", romFile.Roms.Value.Select(e => e.Title)));
            environmentVariables.Add("Romarr_RomFile_GameFileOverviews", string.Join("|", romFile.Roms.Value.Select(e => e.Overview)));
            environmentVariables.Add("Romarr_RomFile_FinaleTypes", string.Join("|", romFile.Roms.Value.Select(e => e.FinaleType)));
            environmentVariables.Add("Romarr_RomFile_Quality", romFile.Quality.Quality.Name);
            environmentVariables.Add("Romarr_RomFile_QualityVersion", romFile.Quality.Revision.Version.ToString());
            environmentVariables.Add("Romarr_RomFile_ReleaseGroup", romFile.ReleaseGroup ?? string.Empty);
            environmentVariables.Add("Romarr_RomFile_SceneName", romFile.SceneName ?? string.Empty);
            environmentVariables.Add("Romarr_RomFile_SourcePath", sourcePath);
            environmentVariables.Add("Romarr_RomFile_SourceFolder", Path.GetDirectoryName(sourcePath));
            environmentVariables.Add("Romarr_Download_Client", message.DownloadClientInfo?.Name ?? string.Empty);
            environmentVariables.Add("Romarr_Download_Client_Type", message.DownloadClientInfo?.Type ?? string.Empty);
            environmentVariables.Add("Romarr_Download_Id", message.DownloadId ?? string.Empty);
            environmentVariables.Add("Romarr_RomFile_MediaInfo_AudioChannels", MediaInfoFormatter.FormatAudioChannels(romFile.MediaInfo.PrimaryAudioStream).ToString());
            environmentVariables.Add("Romarr_RomFile_MediaInfo_AudioCodec", MediaInfoFormatter.FormatAudioCodec(romFile.MediaInfo.PrimaryAudioStream, null));
            environmentVariables.Add("Romarr_RomFile_MediaInfo_AudioLanguages", romFile.MediaInfo.AudioStreams?.Select(l => l.Language).Distinct().ConcatToString(" / "));
            environmentVariables.Add("Romarr_RomFile_MediaInfo_Languages", romFile.MediaInfo.AudioStreams?.Select(l => l.Language).ConcatToString(" / "));
            environmentVariables.Add("Romarr_RomFile_MediaInfo_Height", romFile.MediaInfo.Height.ToString());
            environmentVariables.Add("Romarr_RomFile_MediaInfo_Width", romFile.MediaInfo.Width.ToString());
            environmentVariables.Add("Romarr_RomFile_MediaInfo_Subtitles", romFile.MediaInfo.SubtitleStreams?.Select(l => l.Language).ConcatToString(" / "));
            environmentVariables.Add("Romarr_RomFile_MediaInfo_VideoCodec", MediaInfoFormatter.FormatVideoCodec(romFile.MediaInfo, null));
            environmentVariables.Add("Romarr_RomFile_MediaInfo_VideoDynamicRangeType", MediaInfoFormatter.FormatVideoDynamicRangeType(romFile.MediaInfo));
            environmentVariables.Add("Romarr_RomFile_CustomFormat", string.Join("|", message.RomInfo.CustomFormats));
            environmentVariables.Add("Romarr_RomFile_CustomFormatScore", message.RomInfo.CustomFormatScore.ToString());
            environmentVariables.Add("Romarr_Release_Indexer", message.Release?.Indexer);
            environmentVariables.Add("Romarr_Release_Size", message.Release?.Size.ToString());
            environmentVariables.Add("Romarr_Release_Title", message.Release?.Title);
            environmentVariables.Add("Romarr_Release_ReleaseType", message.Release?.ReleaseType.ToString() ?? string.Empty);

            if (message.OldFiles.Any())
            {
                environmentVariables.Add("Romarr_DeletedRelativePaths", string.Join("|", message.OldFiles.Select(e => e.RomFile.RelativePath)));
                environmentVariables.Add("Romarr_DeletedPaths", string.Join("|", message.OldFiles.Select(e => Path.Combine(game.Path, e.RomFile.RelativePath))));
                environmentVariables.Add("Romarr_DeletedDateAdded", string.Join("|", message.OldFiles.Select(e => e.RomFile.DateAdded)));
                environmentVariables.Add("Romarr_DeletedRecycleBinPaths", string.Join("|", message.OldFiles.Select(e => e.RecycleBinPath ?? string.Empty)));
            }

            ExecuteScript(environmentVariables);
        }

        public override void OnImportComplete(ImportCompleteMessage message)
        {
            var game = message.Game;
            var roms = message.Roms;
            var romFiles = message.RomFiles;
            var sourcePath = message.SourcePath;
            var environmentVariables = new StringDictionary();

            AddInstanceVariables(environmentVariables, "Download");
            AddGameVariables(environmentVariables, game);

            environmentVariables.Add("Romarr_RomFile_Ids", string.Join("|", romFiles.Select(f => f.Id)));
            environmentVariables.Add("Romarr_RomFile_Count", message.RomFiles.Count.ToString());
            environmentVariables.Add("Romarr_RomFile_RelativePaths", string.Join("|", romFiles.Select(f => f.RelativePath)));
            environmentVariables.Add("Romarr_RomFile_Paths", string.Join("|", romFiles.Select(f => Path.Combine(game.Path, f.RelativePath))));
            environmentVariables.Add("Romarr_RomFile_RomIds", string.Join(",", roms.Select(e => e.Id)));
            environmentVariables.Add("Romarr_RomFile_PlatformNumber", roms.First().PlatformNumber.ToString());
            environmentVariables.Add("Romarr_RomFile_RomNumbers", string.Join(",", roms.Select(e => e.FileNumber)));
            environmentVariables.Add("Romarr_RomFile_GameFileAirDates", string.Join(",", roms.Select(e => e.AirDate)));
            environmentVariables.Add("Romarr_RomFile_GameFileAirDatesUtc", string.Join(",", roms.Select(e => e.AirDateUtc)));
            environmentVariables.Add("Romarr_RomFile_RomTitles", string.Join("|", roms.Select(e => e.Title)));
            environmentVariables.Add("Romarr_RomFile_GameFileOverviews", string.Join("|", roms.Select(e => e.Overview)));
            environmentVariables.Add("Romarr_RomFile_FinaleTypes", string.Join("|", roms.Select(e => e.FinaleType)));
            environmentVariables.Add("Romarr_RomFile_Qualities", string.Join("|", romFiles.Select(f => f.Quality.Quality.Name)));
            environmentVariables.Add("Romarr_RomFile_QualityVersions", string.Join("|", romFiles.Select(f => f.Quality.Revision.Version)));
            environmentVariables.Add("Romarr_RomFile_ReleaseGroups", string.Join("|", romFiles.Select(f => f.ReleaseGroup)));
            environmentVariables.Add("Romarr_RomFile_SceneNames", string.Join("|", romFiles.Select(f => f.SceneName)));
            environmentVariables.Add("Romarr_Download_Client", message.DownloadClientInfo?.Name ?? string.Empty);
            environmentVariables.Add("Romarr_Download_Client_Type", message.DownloadClientInfo?.Type ?? string.Empty);
            environmentVariables.Add("Romarr_Download_Id", message.DownloadId ?? string.Empty);
            environmentVariables.Add("Romarr_Release_Group", message.ReleaseGroup ?? string.Empty);
            environmentVariables.Add("Romarr_Release_Quality", message.ReleaseQuality.Quality.Name);
            environmentVariables.Add("Romarr_Release_QualityVersion", message.ReleaseQuality.Revision.Version.ToString());
            environmentVariables.Add("Romarr_Release_Indexer", message.Release?.Indexer ?? string.Empty);
            environmentVariables.Add("Romarr_Release_Size", message.Release?.Size.ToString() ?? string.Empty);
            environmentVariables.Add("Romarr_Release_Title", message.Release?.Title ?? string.Empty);

            // Prefer the release type from the release, otherwise use the first imported file (useful for untracked manual imports)
            environmentVariables.Add("Romarr_Release_ReleaseType", message.Release == null ? message.RomFiles.First().ReleaseType.ToString() : message.Release.ReleaseType.ToString());
            environmentVariables.Add("Romarr_SourcePath", sourcePath);
            environmentVariables.Add("Romarr_SourceFolder", Path.GetDirectoryName(sourcePath));
            environmentVariables.Add("Romarr_DestinationPath", message.DestinationPath);
            environmentVariables.Add("Romarr_DestinationFolder", Path.GetDirectoryName(message.DestinationPath));

            ExecuteScript(environmentVariables);
        }

        public override void OnRename(Game game, List<RenamedRomFile> renamedFiles)
        {
            var environmentVariables = new StringDictionary();

            AddInstanceVariables(environmentVariables, "Rename");
            AddGameVariables(environmentVariables, game);

            environmentVariables.Add("Romarr_RomFile_Ids", string.Join(",", renamedFiles.Select(e => e.RomFile.Id)));
            environmentVariables.Add("Romarr_RomFile_RelativePaths", string.Join("|", renamedFiles.Select(e => e.RomFile.RelativePath)));
            environmentVariables.Add("Romarr_RomFile_Paths", string.Join("|", renamedFiles.Select(e => Path.Combine(game.Path, e.RomFile.RelativePath))));
            environmentVariables.Add("Romarr_RomFile_PreviousRelativePaths", string.Join("|", renamedFiles.Select(e => e.PreviousRelativePath)));
            environmentVariables.Add("Romarr_RomFile_PreviousPaths", string.Join("|", renamedFiles.Select(e => e.PreviousPath)));

            ExecuteScript(environmentVariables);
        }

        public override void OnRomFileDelete(GameFileDeleteMessage deleteMessage)
        {
            var game = deleteMessage.Game;
            var romFile = deleteMessage.RomFile;

            var environmentVariables = new StringDictionary();

            AddInstanceVariables(environmentVariables, "RomFileDelete");
            AddGameVariables(environmentVariables, game);

            environmentVariables.Add("Romarr_RomFile_Id", romFile.Id.ToString());
            environmentVariables.Add("Romarr_RomFile_GameFileCount", romFile.Roms.Value.Count.ToString());
            environmentVariables.Add("Romarr_RomFile_RelativePath", romFile.RelativePath);
            environmentVariables.Add("Romarr_RomFile_Path", Path.Combine(game.Path, romFile.RelativePath));
            environmentVariables.Add("Romarr_RomFile_RomIds", string.Join(",", romFile.Roms.Value.Select(e => e.Id)));
            environmentVariables.Add("Romarr_RomFile_PlatformNumber", romFile.PlatformNumber.ToString());
            environmentVariables.Add("Romarr_RomFile_RomNumbers", string.Join(",", romFile.Roms.Value.Select(e => e.FileNumber)));
            environmentVariables.Add("Romarr_RomFile_GameFileAirDates", string.Join(",", romFile.Roms.Value.Select(e => e.AirDate)));
            environmentVariables.Add("Romarr_RomFile_GameFileAirDatesUtc", string.Join(",", romFile.Roms.Value.Select(e => e.AirDateUtc)));
            environmentVariables.Add("Romarr_RomFile_RomTitles", string.Join("|", romFile.Roms.Value.Select(e => e.Title)));
            environmentVariables.Add("Romarr_RomFile_GameFileOverviews", string.Join("|", romFile.Roms.Value.Select(e => e.Overview)));
            environmentVariables.Add("Romarr_RomFile_Quality", romFile.Quality.Quality.Name);
            environmentVariables.Add("Romarr_RomFile_QualityVersion", romFile.Quality.Revision.Version.ToString());
            environmentVariables.Add("Romarr_RomFile_ReleaseGroup", romFile.ReleaseGroup ?? string.Empty);
            environmentVariables.Add("Romarr_RomFile_SceneName", romFile.SceneName ?? string.Empty);

            ExecuteScript(environmentVariables);
        }

        public override void OnSeriesAdd(SeriesAddMessage message)
        {
            var game = message.Game;
            var environmentVariables = new StringDictionary();

            AddInstanceVariables(environmentVariables, "SeriesAdd");
            AddGameVariables(environmentVariables, game);

            ExecuteScript(environmentVariables);
        }

        public override void OnSeriesDelete(SeriesDeleteMessage deleteMessage)
        {
            var game = deleteMessage.Game;
            var environmentVariables = new StringDictionary();

            AddInstanceVariables(environmentVariables, "SeriesDelete");
            AddGameVariables(environmentVariables, game);

            ExecuteScript(environmentVariables);
        }

        public override void OnHealthIssue(HealthCheck.HealthCheck healthCheck)
        {
            var environmentVariables = new StringDictionary();

            AddInstanceVariables(environmentVariables, "HealthIssue");

            environmentVariables.Add("Romarr_Health_Issue_Level", Enum.GetName(typeof(HealthCheckResult), healthCheck.Type));
            environmentVariables.Add("Romarr_Health_Issue_Message", healthCheck.Message);
            environmentVariables.Add("Romarr_Health_Issue_Type", healthCheck.Source.Name);
            environmentVariables.Add("Romarr_Health_Issue_Wiki", healthCheck.WikiUrl.ToString() ?? string.Empty);

            ExecuteScript(environmentVariables);
        }

        public override void OnHealthRestored(HealthCheck.HealthCheck previousCheck)
        {
            var environmentVariables = new StringDictionary();

            AddInstanceVariables(environmentVariables, "HealthRestored");

            environmentVariables.Add("Romarr_Health_Restored_Level", Enum.GetName(typeof(HealthCheckResult), previousCheck.Type));
            environmentVariables.Add("Romarr_Health_Restored_Message", previousCheck.Message);
            environmentVariables.Add("Romarr_Health_Restored_Type", previousCheck.Source.Name);
            environmentVariables.Add("Romarr_Health_Restored_Wiki", previousCheck.WikiUrl.ToString() ?? string.Empty);

            ExecuteScript(environmentVariables);
        }

        public override void OnApplicationUpdate(ApplicationUpdateMessage updateMessage)
        {
            var environmentVariables = new StringDictionary();

            AddInstanceVariables(environmentVariables, "ApplicationUpdate");

            environmentVariables.Add("Romarr_Update_Message", updateMessage.Message);
            environmentVariables.Add("Romarr_Update_NewVersion", updateMessage.NewVersion.ToString());
            environmentVariables.Add("Romarr_Update_PreviousVersion", updateMessage.PreviousVersion.ToString());

            ExecuteScript(environmentVariables);
        }

        public override void OnManualInteractionRequired(ManualInteractionRequiredMessage message)
        {
            var game = message.Game;
            var environmentVariables = new StringDictionary();

            AddInstanceVariables(environmentVariables, "ManualInteractionRequired");
            AddGameVariables(environmentVariables, game);

            environmentVariables.Add("Romarr_Download_Client", message.DownloadClientInfo?.Name ?? string.Empty);
            environmentVariables.Add("Romarr_Download_Client_Type", message.DownloadClientInfo?.Type ?? string.Empty);
            environmentVariables.Add("Romarr_Download_Id", message.DownloadId ?? string.Empty);
            environmentVariables.Add("Romarr_Download_Size", message.TrackedDownload.DownloadItem.TotalSize.ToString());
            environmentVariables.Add("Romarr_Download_Title", message.TrackedDownload.DownloadItem.Title);

            ExecuteScript(environmentVariables);
        }

        public override ValidationResult Test()
        {
            var failures = new List<ValidationFailure>();

            if (!_diskProvider.FileExists(Settings.Path))
            {
                failures.Add(new RomarrValidationFailure("Path", _localizationService.GetLocalizedString("NotificationsCustomScriptValidationFileDoesNotExist")));
            }

            if (failures.Empty())
            {
                try
                {
                    var environmentVariables = new StringDictionary();
                    environmentVariables.Add("Romarr_EventType", "Test");
                    environmentVariables.Add("Romarr_InstanceName", _configFileProvider.InstanceName);
                    environmentVariables.Add("Romarr_ApplicationUrl", _configService.ApplicationUrl);

                    var processOutput = ExecuteScript(environmentVariables);

                    if (processOutput.ExitCode != 0)
                    {
                        failures.Add(new RomarrValidationFailure(string.Empty, $"Script exited with code: {processOutput.ExitCode}"));
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error(ex);
                    failures.Add(new RomarrValidationFailure(string.Empty, ex.Message));
                }
            }

            return new ValidationResult(failures);
        }

        private ProcessOutput ExecuteScript(StringDictionary environmentVariables)
        {
            _logger.Debug("Executing external script: {0}", Settings.Path);

            var processOutput = _processProvider.StartAndCapture(Settings.Path, Settings.Arguments, environmentVariables);

            _logger.Debug("Executed external script: {0} - Status: {1}", Settings.Path, processOutput.ExitCode);
            _logger.Debug("Script Output: \r\n{0}", string.Join("\r\n", processOutput.Lines));

            return processOutput;
        }

        private bool ValidatePathParent(string possibleParent, string path)
        {
            return possibleParent.IsParentPath(path);
        }

        private List<string> GetTagLabels(Game game)
        {
            if (game == null)
            {
                return new List<string>();
            }

            return _tagRepository.GetTags(game.Tags)
                .Select(s => s.Label)
                .Where(l => l.IsNotNullOrWhiteSpace())
                .OrderBy(l => l)
                .ToList();
        }

        private void AddInstanceVariables(StringDictionary environmentVariables, string eventType)
        {
            environmentVariables.Add("Romarr_EventType", eventType);
            environmentVariables.Add("Romarr_InstanceName", _configFileProvider.InstanceName);
            environmentVariables.Add("Romarr_ApplicationUrl", _configService.ApplicationUrl);
        }

        private void AddGameVariables(StringDictionary environmentVariables, Game game)
        {
            environmentVariables.Add("Romarr_Series_Id", game.Id.ToString());
            environmentVariables.Add("Romarr_Series_Title", game.Title);
            environmentVariables.Add("Romarr_Series_TitleSlug", game.TitleSlug);
            environmentVariables.Add("Romarr_Series_Path", game.Path);
            environmentVariables.Add("Romarr_Series_IgdbId", game.IgdbId.ToString());
            environmentVariables.Add("Romarr_Series_RawgId", game.RawgId.ToString());
            environmentVariables.Add("Romarr_Series_TmdbId", game.TmdbId.ToString());
            environmentVariables.Add("Romarr_Series_ImdbId", game.ImdbId ?? string.Empty);
            environmentVariables.Add("Romarr_Series_Type", game.GameType.ToString());
            environmentVariables.Add("Romarr_Series_Year", game.Year.ToString());
            environmentVariables.Add("Romarr_Series_OriginalCountry", game.OriginalCountry);
            environmentVariables.Add("Romarr_Series_OriginalLanguage", IsoLanguages.Get(game.OriginalLanguage).ThreeLetterCode);
            environmentVariables.Add("Romarr_Series_Genres", string.Join("|", game.Genres));
            environmentVariables.Add("Romarr_Series_Tags", string.Join("|", GetTagLabels(game)));
        }
    }
}
