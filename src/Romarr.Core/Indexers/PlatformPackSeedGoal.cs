using Romarr.Core.Annotations;

namespace Romarr.Core.Indexers;

public enum PlatformPackSeedGoal
{
    [FieldOption(Label = "IndexerSettingsPlatformPackSeedGoalUseStandardGoals")]
    UseStandardSeedGoal = 0,
    [FieldOption(Label = "IndexerSettingsPlatformPackSeedGoalUsePlatformPackGoals")]
    UsePlatformPackSeedGoal = 1
}
