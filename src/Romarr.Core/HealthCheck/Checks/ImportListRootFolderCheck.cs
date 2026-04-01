using System.Collections.Generic;
using System.Linq;
using Romarr.Common.Disk;
using Romarr.Common.Extensions;
using Romarr.Core.Datastore.Events;
using Romarr.Core.ImportLists;
using Romarr.Core.Localization;
using Romarr.Core.MediaFiles.Events;
using Romarr.Core.RootFolders;
using Romarr.Core.ThingiProvider.Events;
using Romarr.Core.Games.Events;

namespace Romarr.Core.HealthCheck.Checks
{
    [CheckOn(typeof(ProviderUpdatedEvent<IImportList>))]
    [CheckOn(typeof(ProviderDeletedEvent<IImportList>))]
    [CheckOn(typeof(ModelEvent<RootFolder>))]
    [CheckOn(typeof(GameDeletedEvent))]
    [CheckOn(typeof(GameMovedEvent))]
    [CheckOn(typeof(FileImportedEvent), CheckOnCondition.FailedOnly)]
    [CheckOn(typeof(GameFileImportFailedEvent), CheckOnCondition.SuccessfulOnly)]
    public class ImportListRootFolderCheck : HealthCheckBase
    {
        private readonly IImportListFactory _importListFactory;
        private readonly IDiskProvider _diskProvider;
        private readonly IRootFolderService _rootFolderService;

        public ImportListRootFolderCheck(IImportListFactory importListFactory, IDiskProvider diskProvider, IRootFolderService rootFolderService, ILocalizationService localizationService)
            : base(localizationService)
        {
            _importListFactory = importListFactory;
            _diskProvider = diskProvider;
            _rootFolderService = rootFolderService;
        }

        public override HealthCheck Check()
        {
            var importLists = _importListFactory.All();
            var rootFolders = _rootFolderService.All();

            var missingRootFolders = new Dictionary<string, List<ImportListDefinition>>();

            foreach (var importList in importLists)
            {
                var rootFolderPath = importList.RootFolderPath;

                if (missingRootFolders.ContainsKey(rootFolderPath))
                {
                    missingRootFolders[rootFolderPath].Add(importList);

                    continue;
                }

                if (rootFolderPath.IsNullOrWhiteSpace() ||
                    !rootFolderPath.IsPathValid(PathValidationType.CurrentOs) ||
                    !rootFolders.Any(r => r.Path.PathEquals(rootFolderPath)) ||
                    !_diskProvider.FolderExists(rootFolderPath))
                {
                    missingRootFolders.Add(rootFolderPath, new List<ImportListDefinition> { importList });
                }
            }

            if (missingRootFolders.Any())
            {
                if (missingRootFolders.Count == 1)
                {
                    var missingRootFolder = missingRootFolders.First();

                    return new HealthCheck(GetType(),
                        HealthCheckResult.Error,
                        HealthCheckReason.ImportListRootFolderMissing,
                        _localizationService.GetLocalizedString("ImportListRootFolderMissingRootHealthCheckMessage", new Dictionary<string, object>
                        {
                            { "rootFolderInfo", FormatRootFolder(missingRootFolder.Key, missingRootFolder.Value) }
                        }),
                        "#import-list-missing-root-folder");
                }

                return new HealthCheck(GetType(),
                    HealthCheckResult.Error,
                    HealthCheckReason.ImportListRootFolderMultipleMissing,
                    _localizationService.GetLocalizedString("ImportListRootFolderMultipleMissingRootsHealthCheckMessage", new Dictionary<string, object>
                    {
                        { "rootFoldersInfo", string.Join(" | ", missingRootFolders.Select(m => FormatRootFolder(m.Key, m.Value))) }
                    }),
                    "#import-list-missing-root-folder");
            }

            return new HealthCheck(GetType());
        }

        private string FormatRootFolder(string rootFolderPath, List<ImportListDefinition> importLists)
        {
            return $"{rootFolderPath} ({string.Join(", ", importLists.Select(l => l.Name))})";
        }
    }
}
