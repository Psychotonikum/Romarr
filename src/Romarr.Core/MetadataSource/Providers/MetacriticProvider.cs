using FluentValidation;
using Romarr.Core.Annotations;
using Romarr.Core.ThingiProvider;
using Romarr.Core.Validation;

namespace Romarr.Core.MetadataSource.Providers
{
    public class MetacriticProviderSettingsValidator : AbstractValidator<MetacriticProviderSettings>
    {
    }

    public class MetacriticProviderSettings : IProviderConfig
    {
        private static readonly MetacriticProviderSettingsValidator Validator = new();

        [FieldDefinition(0, Label = "Use as Rating Source", Type = FieldType.Checkbox, HelpText = "Use Metacritic scores instead of IGDB ratings")]
        public bool UseAsRatingSource { get; set; }

        public RomarrValidationResult Validate()
        {
            return new RomarrValidationResult(Validator.Validate(this));
        }
    }

    public class MetacriticProvider : MetadataSourceProviderBase<MetacriticProviderSettings>
    {
        public MetacriticProvider(NLog.Logger logger)
            : base(logger)
        {
        }

        public override string Name => "Metacritic";
        public override bool SupportsSearch => false;
        public override bool SupportsCalendar => true;
        public override bool SupportsMetadataDownload => false;
    }
}
