using FluentValidation;
using Romarr.Core.Annotations;
using Romarr.Core.Games;
using Romarr.Core.Validation;

namespace Romarr.Core.AutoTagging.Specifications
{
    public class GameTypeSpecificationValidator : AbstractValidator<GameTypeSpecification>
    {
        public GameTypeSpecificationValidator()
        {
            RuleFor(c => (GameTypes)c.Value).IsInEnum();
        }
    }

    public class GameTypeSpecification : AutoTaggingSpecificationBase
    {
        private static readonly GameTypeSpecificationValidator Validator = new GameTypeSpecificationValidator();

        public override int Order => 2;
        public override string ImplementationName => "Game Type";

        [FieldDefinition(1, Label = "AutoTaggingSpecificationGameType", Type = FieldType.Select, SelectOptions = typeof(GameTypes))]
        public int Value { get; set; }

        protected override bool IsSatisfiedByWithoutNegate(Game game)
        {
            return (int)game.GameType == Value;
        }

        public override RomarrValidationResult Validate()
        {
            return new RomarrValidationResult(Validator.Validate(this));
        }
    }
}
