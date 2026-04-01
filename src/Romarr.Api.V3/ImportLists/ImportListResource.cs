using System;
using Romarr.Core.ImportLists;
using Romarr.Core.Games;

namespace Romarr.Api.V3.ImportLists
{
    public class ImportListResource : ProviderResource<ImportListResource>
    {
        public bool EnableAutomaticAdd { get; set; }
        public bool SearchForMissingGameFiles { get; set; }
        public MonitorTypes ShouldMonitor { get; set; }
        public NewItemMonitorTypes MonitorNewItems { get; set; }
        public string RootFolderPath { get; set; }
        public int QualityProfileId { get; set; }
        public GameTypes GameType { get; set; }
        public bool PlatformFolder { get; set; }
        public ImportListType ListType { get; set; }
        public int ListOrder { get; set; }
        public TimeSpan MinRefreshInterval { get; set; }
    }

    public class ImportListResourceMapper : ProviderResourceMapper<ImportListResource, ImportListDefinition>
    {
        public override ImportListResource ToResource(ImportListDefinition definition)
        {
            if (definition == null)
            {
                return null;
            }

            var resource = base.ToResource(definition);

            resource.EnableAutomaticAdd = definition.EnableAutomaticAdd;
            resource.SearchForMissingGameFiles = definition.SearchForMissingGameFiles;
            resource.ShouldMonitor = definition.ShouldMonitor;
            resource.MonitorNewItems = definition.MonitorNewItems;
            resource.RootFolderPath = definition.RootFolderPath;
            resource.QualityProfileId = definition.QualityProfileId;
            resource.GameType = definition.GameType;
            resource.PlatformFolder = definition.PlatformFolder;
            resource.ListType = definition.ListType;
            resource.ListOrder = (int)definition.ListType;
            resource.MinRefreshInterval = definition.MinRefreshInterval;

            return resource;
        }

        public override ImportListDefinition ToModel(ImportListResource resource, ImportListDefinition existingDefinition)
        {
            if (resource == null)
            {
                return null;
            }

            var definition = base.ToModel(resource, existingDefinition);

            definition.EnableAutomaticAdd = resource.EnableAutomaticAdd;
            definition.SearchForMissingGameFiles = resource.SearchForMissingGameFiles;
            definition.ShouldMonitor = resource.ShouldMonitor;
            definition.MonitorNewItems = resource.MonitorNewItems;
            definition.RootFolderPath = resource.RootFolderPath;
            definition.QualityProfileId = resource.QualityProfileId;
            definition.GameType = resource.GameType;
            definition.PlatformFolder = resource.PlatformFolder;
            definition.ListType = resource.ListType;
            definition.MinRefreshInterval = resource.MinRefreshInterval;

            return definition;
        }
    }
}
