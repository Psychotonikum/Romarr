using FluentValidation;
using Romarr.Core.Annotations;
using Romarr.Core.Validation;

namespace Romarr.Core.Notifications.Prowl
{
    public class ProwlSettingsValidator : AbstractValidator<ProwlSettings>
    {
        public ProwlSettingsValidator()
        {
            RuleFor(c => c.ApiKey).NotEmpty();
        }
    }

    public class ProwlSettings : NotificationSettingsBase<ProwlSettings>
    {
        private static readonly ProwlSettingsValidator Validator = new();

        [FieldDefinition(0, Label = "ApiKey", Privacy = PrivacyLevel.ApiKey, HelpLink = "https://www.prowlapp.com/api_settings.php")]
        public string ApiKey { get; set; }

        [FieldDefinition(1, Label = "Priority", Type = FieldType.Select, SelectOptions = typeof(ProwlPriority))]
        public int Priority { get; set; }

        public bool IsValid => !string.IsNullOrWhiteSpace(ApiKey) && Priority >= -2 && Priority <= 2;

        public override RomarrValidationResult Validate()
        {
            return new RomarrValidationResult(Validator.Validate(this));
        }
    }
}
