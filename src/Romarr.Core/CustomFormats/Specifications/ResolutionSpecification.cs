using FluentValidation;
using Romarr.Core.Annotations;
using Romarr.Core.Parser;
using Romarr.Core.Validation;

namespace Romarr.Core.CustomFormats
{
    public class ResolutionSpecificationValidator : AbstractValidator<ResolutionSpecification>
    {
        public ResolutionSpecificationValidator()
        {
            RuleFor(c => c.Value).NotEmpty();
        }
    }

    public class ResolutionSpecification : CustomFormatSpecificationBase
    {
        private static readonly ResolutionSpecificationValidator Validator = new ResolutionSpecificationValidator();

        public override int Order => 6;
        public override string ImplementationName => "Resolution";

        [FieldDefinition(1, Label = "CustomFormatsSpecificationResolution", Type = FieldType.Select, SelectOptions = typeof(Resolution))]
        public int Value { get; set; }

        protected override bool IsSatisfiedByWithoutNegate(CustomFormatInput input)
        {
            return (input.RomInfo?.Quality?.Quality?.Resolution ?? (int)Resolution.Unknown) == Value;
        }

        public override RomarrValidationResult Validate()
        {
            return new RomarrValidationResult(Validator.Validate(this));
        }
    }
}
