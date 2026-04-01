using Romarr.Core.Games;
using Romarr.Core.Validation;

namespace Romarr.Core.AutoTagging.Specifications
{
    public interface IAutoTaggingSpecification
    {
        int Order { get; }
        string ImplementationName { get; }
        string Name { get; set; }
        bool Negate { get; set; }
        bool Required { get; set; }
        RomarrValidationResult Validate();

        IAutoTaggingSpecification Clone();
        bool IsSatisfiedBy(Game game);
    }
}
