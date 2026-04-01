using Romarr.Common.Exceptions;

namespace Romarr.Core.ThingiProvider
{
    public class ConfigContractNotFoundException : RomarrException
    {
        public ConfigContractNotFoundException(string contract)
            : base("Couldn't find config contract " + contract)
        {
        }
    }
}
