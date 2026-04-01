using System.Collections.Generic;
using System.Linq;
using Romarr.Common.Disk;
using Romarr.Common.Extensions;
using Romarr.Core.Localization;
using Romarr.Core.MediaFiles.Events;
using Romarr.Core.RootFolders;
using Romarr.Core.Games;
using Romarr.Core.Games.Events;

namespace Romarr.Core.HealthCheck.Checks
{
    [CheckOn(typeof(GameDeletedEvent))]
    [CheckOn(typeof(GameMovedEvent))]
    [CheckOn(typeof(FileImportedEvent), CheckOnCondition.FailedOnly)]
    [CheckOn(typeof(GameFileImportFailedEvent), CheckOnCondition.SuccessfulOnly)]
    public class RootFolderCheck : HealthCheckBase
    {
        private readonly IGameService _gameService;
        private readonly IDiskProvider _diskProvider;
        private readonly IRootFolderService _rootFolderService;

        public RootFolderCheck(IGameService seriesService, IDiskProvider diskProvider, IRootFolderService rootFolderService, ILocalizationService localizationService)
            : base(localizationService)
        {
            _gameService = seriesService;
            _diskProvider = diskProvider;
            _rootFolderService = rootFolderService;
        }

        public override HealthCheck Check()
        {
            var rootFolders = _gameService.GetAllGamePaths()
                .Select(s => _rootFolderService.GetBestRootFolderPath(s.Value))
                .Distinct()
                .ToList();

            var missingRootFolders = rootFolders.Where(s => !s.IsPathValid(PathValidationType.CurrentOs) || !_diskProvider.FolderExists(s))
                .ToList();

            if (missingRootFolders.Any())
            {
                if (missingRootFolders.Count == 1)
                {
                    return new HealthCheck(GetType(),
                        HealthCheckResult.Error,
                        HealthCheckReason.RootFolderMissing,
                        _localizationService.GetLocalizedString(
                            "RootFolderMissingHealthCheckMessage",
                            new Dictionary<string, object>
                            {
                                { "rootFolderPath", missingRootFolders.First() }
                            }),
                        "#missing-root-folder");
                }

                return new HealthCheck(GetType(),
                    HealthCheckResult.Error,
                    HealthCheckReason.RootFolderMultipleMissing,
                    _localizationService.GetLocalizedString(
                        "RootFolderMultipleMissingHealthCheckMessage",
                        new Dictionary<string, object>
                        {
                            { "rootFolderPaths", string.Join(" | ", missingRootFolders) }
                        }),
                    "#missing-root-folder");
            }

            var emptyRootFolders = rootFolders
                .Where(r => _diskProvider.FolderEmpty(r))
                .ToList();

            if (emptyRootFolders.Any())
            {
                if (emptyRootFolders.Count == 1)
                {
                    return new HealthCheck(GetType(),
                        HealthCheckResult.Warning,
                        HealthCheckReason.RootFolderEmpty,
                        _localizationService.GetLocalizedString(
                            "RootFolderEmptyHealthCheckMessage",
                            new Dictionary<string, object>
                            {
                                { "rootFolderPath", emptyRootFolders.First() }
                            }),
                        "#empty-root-folder");
                }

                return new HealthCheck(GetType(),
                    HealthCheckResult.Warning,
                    HealthCheckReason.RootFolderEmpty,
                    _localizationService.GetLocalizedString(
                        "RootFolderMultipleEmptyHealthCheckMessage",
                        new Dictionary<string, object>
                        {
                            { "rootFolderPaths", string.Join(" | ", emptyRootFolders) }
                        }),
                    "#empty-root-folder");
            }

            return new HealthCheck(GetType());
        }
    }
}
