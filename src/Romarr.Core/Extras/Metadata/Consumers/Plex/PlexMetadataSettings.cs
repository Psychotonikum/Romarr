using FluentValidation;
using Romarr.Core.Annotations;
using Romarr.Core.ThingiProvider;
using Romarr.Core.Validation;

namespace Romarr.Core.Extras.Metadata.Consumers.Plex
{
    public class PlexMetadataSettingsValidator : AbstractValidator<PlexMetadataSettings>
    {
    }

    public class PlexMetadataSettings : IProviderConfig
    {
        private static readonly PlexMetadataSettingsValidator Validator = new PlexMetadataSettingsValidator();

        public PlexMetadataSettings()
        {
            SeriesPlexMatchFile = true;
        }

        [FieldDefinition(0, Label = "MetadataPlexSettingsSeriesPlexMatchFile", Type = FieldType.Checkbox, Section = MetadataSectionType.Metadata, HelpText = "MetadataPlexSettingsSeriesPlexMatchFileHelpText")]
        public bool SeriesPlexMatchFile { get; set; }

        [FieldDefinition(0, Label = "MetadataPlexSettingsGameFileMappings", Type = FieldType.Checkbox, Section = MetadataSectionType.Metadata, HelpText = "MetadataPlexSettingsGameFileMappingsHelpText")]
        public bool GameFileMappings { get; set; }

        public RomarrValidationResult Validate()
        {
            return new RomarrValidationResult(Validator.Validate(this));
        }
    }
}
