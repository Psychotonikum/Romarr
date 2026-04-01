using Romarr.Core.Validation;

namespace Romarr.Core.ThingiProvider
{
    public interface IProviderConfig
    {
        RomarrValidationResult Validate();
    }
}
