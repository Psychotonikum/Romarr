using System;
using FluentValidation;
using Romarr.Core.Annotations;
using Romarr.Core.Parser.Model;
using Romarr.Core.Validation;

namespace Romarr.Core.CustomFormats
{
    public class PlatformPackSpecificationValidator : AbstractValidator<ReleaseTypeSpecification>
    {
        public PlatformPackSpecificationValidator()
        {
            RuleFor(c => c.Value).Custom((releaseType, context) =>
            {
                if (!Enum.IsDefined(typeof(ReleaseType), releaseType))
                {
                    context.AddFailure($"Invalid release type condition value: {releaseType}");
                }
            });
        }
    }

    public class ReleaseTypeSpecification : CustomFormatSpecificationBase
    {
        private static readonly PlatformPackSpecificationValidator Validator = new();

        public override int Order => 10;
        public override string ImplementationName => "Release Type";

        [FieldDefinition(1, Label = "ReleaseType", Type = FieldType.Select, SelectOptions = typeof(ReleaseType))]
        public int Value { get; set; }

        protected override bool IsSatisfiedByWithoutNegate(CustomFormatInput input)
        {
            return input.ReleaseType == (ReleaseType)Value;
        }

        public override RomarrValidationResult Validate()
        {
            return new RomarrValidationResult(Validator.Validate(this));
        }
    }
}
