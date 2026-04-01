using FluentValidation;
using Romarr.Core.Annotations;
using Romarr.Core.Validation;

namespace Romarr.Core.Notifications.Synology
{
    public class SynologyIndexerSettingsValidator : AbstractValidator<SynologyIndexerSettings>
    {
    }

    public class SynologyIndexerSettings : NotificationSettingsBase<SynologyIndexerSettings>
    {
        private static readonly SynologyIndexerSettingsValidator Validator = new();

        public SynologyIndexerSettings()
        {
            UpdateLibrary = true;
        }

        [FieldDefinition(0, Label = "NotificationsSettingsUpdateLibrary", Type = FieldType.Checkbox, HelpText = "NotificationsSynologySettingsUpdateLibraryHelpText")]
        public bool UpdateLibrary { get; set; }

        public override RomarrValidationResult Validate()
        {
            return new RomarrValidationResult(Validator.Validate(this));
        }
    }
}
