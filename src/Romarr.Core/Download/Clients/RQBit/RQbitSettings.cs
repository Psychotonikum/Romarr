using FluentValidation;
using Romarr.Core.Annotations;
using Romarr.Core.Validation;

namespace Romarr.Core.Download.Clients.RQBit
{
    public class RQbitSettings : DownloadClientSettingsBase<RQbitSettings>
    {
        private static readonly RQbitSettingsValidator Validator = new();

        public RQbitSettings()
        {
            Host = "localhost";
            Port = 3030;
            UrlBase = "/";
        }

        [FieldDefinition(0, Label = "Host", Type = FieldType.Textbox)]
        public string Host { get; set; }

        [FieldDefinition(1, Label = "Port", Type = FieldType.Textbox)]
        public int Port { get; set; }

        [FieldDefinition(2, Label = "UseSsl", Type = FieldType.Checkbox)]
        [FieldToken(TokenField.HelpText, "DownloadClientRQbitSettingsUseSslHelpText")]
        public bool UseSsl { get; set; }

        [FieldDefinition(3, Label = "UrlBase", Type = FieldType.Textbox, Advanced = true)]
        [FieldToken(TokenField.HelpText, "DownloadClientRQbitSettingsUrlBaseHelpText")]
        public string UrlBase { get; set; }

        public override RomarrValidationResult Validate()
        {
            return new RomarrValidationResult(Validator.Validate(this));
        }
    }

    public class RQbitSettingsValidator : AbstractValidator<RQbitSettings>
    {
        public RQbitSettingsValidator()
        {
            RuleFor(c => c.Host).ValidHost();
            RuleFor(c => c.Port).InclusiveBetween(1, 65535);

            RuleFor(c => c.UrlBase).ValidUrlBase();
        }
    }
}
