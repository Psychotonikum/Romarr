using Romarr.Core.Datastore;

namespace Romarr.Core.Games
{
    public class MonitoringOptions : IEmbeddedDocument
    {
        public bool IgnoreGameFilesWithFiles { get; set; }
        public bool IgnoreGameFilesWithoutFiles { get; set; }
        public MonitorTypes Monitor { get; set; }
    }

    public enum MonitorTypes
    {
        Unknown,
        All,
        Future,
        Missing,
        Existing,
        FirstPlatform,
        LastPlatform,
        None,
        Skip,
        BaseGame,
        AllDlcs,
        LatestUpdate,
        AllAdditional
    }

    public enum NewItemMonitorTypes
    {
        All,
        None
    }
}
