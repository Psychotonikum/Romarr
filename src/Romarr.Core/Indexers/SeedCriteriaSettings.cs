using Equ;
using FluentValidation;
using Romarr.Core.Annotations;
using Romarr.Core.Validation;

namespace Romarr.Core.Indexers
{
    public class SeedCriteriaSettingsValidator : AbstractValidator<SeedCriteriaSettings>
    {
        public SeedCriteriaSettingsValidator(double seedRatioMinimum = 0.0, int seedTimeMinimum = 0, int platformPackSeedTimeMinimum = 0)
        {
            RuleFor(c => c.SeedRatio).GreaterThan(0.0)
                .When(c => c.SeedRatio.HasValue)
                .AsWarning().WithMessage("Should be greater than zero");

            RuleFor(c => c.SeedTime).GreaterThan(0)
                .When(c => c.SeedTime.HasValue)
                .AsWarning().WithMessage("Should be greater than zero");

            RuleFor(c => c.PlatformPackSeedRatio).GreaterThan(0.0)
                .When(c => c.PlatformPackSeedRatio.HasValue)
                .AsWarning().WithMessage("Should be greater than zero");

            RuleFor(c => c.PlatformPackSeedTime).GreaterThan(0)
                .When(c => c.PlatformPackSeedTime.HasValue)
                .AsWarning().WithMessage("Should be greater than zero");

            if (seedRatioMinimum != 0.0)
            {
                RuleFor(c => c.SeedRatio).GreaterThanOrEqualTo(seedRatioMinimum)
                    .When(c => c.SeedRatio > 0.0)
                    .AsWarning()
                    .WithMessage($"Under {seedRatioMinimum} leads to H&R");

                RuleFor(c => c.PlatformPackSeedRatio).GreaterThanOrEqualTo(seedRatioMinimum)
                    .When(c => c.PlatformPackSeedRatio > 0.0)
                    .AsWarning()
                    .WithMessage($"Under {seedRatioMinimum} leads to H&R");
            }

            if (seedTimeMinimum != 0)
            {
                RuleFor(c => c.SeedTime).GreaterThanOrEqualTo(seedTimeMinimum)
                    .When(c => c.SeedTime > 0)
                    .AsWarning()
                    .WithMessage($"Under {seedTimeMinimum} leads to H&R");
            }

            if (platformPackSeedTimeMinimum != 0)
            {
                RuleFor(c => c.PlatformPackSeedTime).GreaterThanOrEqualTo(platformPackSeedTimeMinimum)
                    .When(c => c.PlatformPackSeedTime > 0)
                    .AsWarning()
                    .WithMessage($"Under {platformPackSeedTimeMinimum} leads to H&R");
            }
        }
    }

    public class SeedCriteriaSettings : PropertywiseEquatable<SeedCriteriaSettings>
    {
        [FieldDefinition(0, Type = FieldType.Number, Label = "IndexerSettingsSeedRatio", HelpText = "IndexerSettingsSeedRatioHelpText")]
        public double? SeedRatio { get; set; }

        [FieldDefinition(1, Type = FieldType.Number, Label = "IndexerSettingsSeedTime", Unit = "minutes", HelpText = "IndexerSettingsSeedTimeHelpText", Advanced = true)]
        public int? SeedTime { get; set; }

        [FieldDefinition(2, Type = FieldType.Select, Label = "IndexerSettingsPlatformPackSeedGoal", SelectOptions = typeof(PlatformPackSeedGoal), HelpText = "IndexerSettingsPlatformPackSeedGoalHelpText", Advanced = true)]
        public int PlatformPackSeedGoal { get; set; }

        [FieldDefinition(3, Type = FieldType.Number, Label = "IndexerSettingsPlatformPackSeedRatio", HelpText = "IndexerSettingsPlatformPackSeedRatioHelpText", Advanced = true)]
        public double? PlatformPackSeedRatio { get; set; }

        [FieldDefinition(4, Type = FieldType.Number, Label = "IndexerSettingsPlatformPackSeedTime", Unit = "minutes", HelpText = "IndexerSettingsPlatformPackSeedTimeHelpText", Advanced = true)]
        public int? PlatformPackSeedTime { get; set; }
    }
}
