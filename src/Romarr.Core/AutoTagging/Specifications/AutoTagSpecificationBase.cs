using Romarr.Core.Games;
using Romarr.Core.Validation;

namespace Romarr.Core.AutoTagging.Specifications
{
    public abstract class AutoTaggingSpecificationBase : IAutoTaggingSpecification
    {
        public abstract int Order { get; }
        public abstract string ImplementationName { get; }

        public string Name { get; set; }
        public bool Negate { get; set; }
        public bool Required { get; set; }

        public IAutoTaggingSpecification Clone()
        {
            return (IAutoTaggingSpecification)MemberwiseClone();
        }

        public abstract RomarrValidationResult Validate();

        public bool IsSatisfiedBy(Game game)
        {
            var match = IsSatisfiedByWithoutNegate(game);

            if (Negate)
            {
                match = !match;
            }

            return match;
        }

        protected abstract bool IsSatisfiedByWithoutNegate(Game game);
    }
}
