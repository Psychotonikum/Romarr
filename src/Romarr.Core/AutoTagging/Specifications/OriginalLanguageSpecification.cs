using FluentValidation;
using Romarr.Core.Annotations;
using Romarr.Core.Languages;
using Romarr.Core.Games;
using Romarr.Core.Validation;

namespace Romarr.Core.AutoTagging.Specifications
{
    public class OriginalLanguageSpecificationValidator : AbstractValidator<OriginalLanguageSpecification>
    {
        public OriginalLanguageSpecificationValidator()
        {
            RuleFor(c => c.Value).GreaterThanOrEqualTo(0);
        }
    }

    public class OriginalLanguageSpecification : AutoTaggingSpecificationBase
    {
        private static readonly OriginalLanguageSpecificationValidator Validator = new();

        public override int Order => 1;
        public override string ImplementationName => "Original Language";

        [FieldDefinition(1, Label = "AutoTaggingSpecificationOriginalLanguage", Type = FieldType.Select, SelectOptions = typeof(OriginalLanguageFieldConverter))]
        public int Value { get; set; }

        protected override bool IsSatisfiedByWithoutNegate(Game game)
        {
            return Value == game.OriginalLanguage.Id;
        }

        public override RomarrValidationResult Validate()
        {
            return new RomarrValidationResult(Validator.Validate(this));
        }
    }
}
