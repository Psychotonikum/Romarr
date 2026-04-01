using System;
using FluentValidation;
using Romarr.Core.Annotations;
using Romarr.Core.Validation;

namespace Romarr.Core.Notifications.Trakt
{
    public class TraktSettingsValidator : AbstractValidator<TraktSettings>
    {
        public TraktSettingsValidator()
        {
            RuleFor(c => c.AccessToken).NotEmpty();
            RuleFor(c => c.RefreshToken).NotEmpty();
            RuleFor(c => c.Expires).NotEmpty();
        }
    }

    public class TraktSettings : NotificationSettingsBase<TraktSettings>
    {
        private static readonly TraktSettingsValidator Validator = new();

        public TraktSettings()
        {
            SignIn = "startOAuth";
        }

        [FieldDefinition(0, Label = "NotificationsTraktSettingsAccessToken", Type = FieldType.Textbox, Hidden = HiddenType.Hidden)]
        public string AccessToken { get; set; }

        [FieldDefinition(1, Label = "NotificationsTraktSettingsRefreshToken", Type = FieldType.Textbox, Hidden = HiddenType.Hidden)]
        public string RefreshToken { get; set; }

        [FieldDefinition(2, Label = "NotificationsTraktSettingsExpires", Type = FieldType.Textbox, Hidden = HiddenType.Hidden)]
        public DateTime Expires { get; set; }

        [FieldDefinition(3, Label = "NotificationsTraktSettingsAuthUser", Type = FieldType.Textbox, Hidden = HiddenType.Hidden)]
        public string AuthUser { get; set; }

        [FieldDefinition(4, Label = "NotificationsTraktSettingsAuthenticateWithTrakt", Type = FieldType.OAuth)]
        public string SignIn { get; set; }

        public override RomarrValidationResult Validate()
        {
            return new RomarrValidationResult(Validator.Validate(this));
        }
    }
}
