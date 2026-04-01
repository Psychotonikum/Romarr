using Romarr.Core.Validation;

namespace Romarr.Core.ThingiProvider
{
    public class NullConfig : IProviderConfig
    {
        public static readonly NullConfig Instance = new NullConfig();

        public RomarrValidationResult Validate()
        {
            return new RomarrValidationResult();
        }
    }
}
