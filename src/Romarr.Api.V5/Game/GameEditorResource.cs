using Romarr.Core.Games;

namespace Romarr.Api.V5.Game;

public class GameEditorResource
{
    public List<int> GameIds { get; set; } = [];
    public bool? Monitored { get; set; }
    public NewItemMonitorTypes? MonitorNewItems { get; set; }
    public int? QualityProfileId { get; set; }
    public GameTypes? GameType { get; set; }
    public bool? PlatformFolder { get; set; }
    public string? RootFolderPath { get; set; }
    public List<int> Tags { get; set; } = [];
    public ApplyTags ApplyTags { get; set; }
    public bool MoveFiles { get; set; }
    public bool DeleteFiles { get; set; }
    public bool AddImportListExclusion { get; set; }
}
