using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using Romarr.Common.Extensions;
using Romarr.Core.Annotations;
using Romarr.Core.Games;
using Romarr.Core.Validation;

namespace Romarr.Core.AutoTagging.Specifications
{
    public class NetworkSpecificationValidator : AbstractValidator<NetworkSpecification>
    {
        public NetworkSpecificationValidator()
        {
            RuleFor(c => c.Value).NotEmpty();
        }
    }

    public class NetworkSpecification : AutoTaggingSpecificationBase
    {
        private static readonly NetworkSpecificationValidator Validator = new();

        public override int Order => 1;
        public override string ImplementationName => "Network";

        [FieldDefinition(1, Label = "AutoTaggingSpecificationNetwork", Type = FieldType.Tag)]
        public IEnumerable<string> Value { get; set; }

        protected override bool IsSatisfiedByWithoutNegate(Game game)
        {
            return Value.Any(network => game.Network.EqualsIgnoreCase(network));
        }

        public override RomarrValidationResult Validate()
        {
            return new RomarrValidationResult(Validator.Validate(this));
        }
    }
}
