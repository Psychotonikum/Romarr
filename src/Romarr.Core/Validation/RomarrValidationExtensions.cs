using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using FluentValidation.Results;

namespace Romarr.Core.Validation
{
    public static class RomarrValidationExtensions
    {
        public static RomarrValidationResult Filter(this RomarrValidationResult result, params string[] fields)
        {
            var failures = result.Failures.Where(v => fields.Contains(v.PropertyName)).ToArray();

            return new RomarrValidationResult(failures);
        }

        public static void ThrowOnError(this RomarrValidationResult result)
        {
            if (!result.IsValid)
            {
                throw new ValidationException(result.Errors);
            }
        }

        public static bool HasErrors(this List<ValidationFailure> list)
        {
            return list.Any(item => item is not RomarrValidationFailure { IsWarning: true });
        }
    }
}
