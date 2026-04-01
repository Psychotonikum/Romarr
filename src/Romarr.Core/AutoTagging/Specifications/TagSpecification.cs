using FluentValidation;
using Romarr.Core.Annotations;
using Romarr.Core.Games;
using Romarr.Core.Validation;

namespace Romarr.Core.AutoTagging.Specifications
{
    public class TagSpecificationValidator : AbstractValidator<TagSpecification>
    {
        public TagSpecificationValidator()
        {
            RuleFor(c => c.Value).GreaterThan(0);
        }
    }

    public class TagSpecification : AutoTaggingSpecificationBase
    {
        private static readonly TagSpecificationValidator Validator = new();

        public override int Order => 1;
        public override string ImplementationName => "Tag";

        [FieldDefinition(1, Label = "AutoTaggingSpecificationTag", Type = FieldType.SeriesTag)]
        public int Value { get; set; }

        protected override bool IsSatisfiedByWithoutNegate(Game game)
        {
            return game.Tags.Contains(Value);
        }

        public override RomarrValidationResult Validate()
        {
            return new RomarrValidationResult(Validator.Validate(this));
        }
    }
}
