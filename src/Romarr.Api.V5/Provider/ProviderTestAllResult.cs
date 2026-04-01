using FluentValidation.Results;
using Romarr.Common.Extensions;

namespace Romarr.Api.V5.Provider
{
    public class ProviderTestAllResult
    {
        public int Id { get; set; }
        public bool IsValid => ValidationFailures.Empty();
        public List<ValidationFailure> ValidationFailures { get; set; }

        public ProviderTestAllResult()
        {
            ValidationFailures = new List<ValidationFailure>();
        }
    }
}
