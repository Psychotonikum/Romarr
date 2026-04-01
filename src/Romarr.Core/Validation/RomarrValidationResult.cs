using System.Collections.Generic;
using System.Linq;
using FluentValidation.Results;
using Romarr.Common.Extensions;

namespace Romarr.Core.Validation
{
    public class RomarrValidationResult : ValidationResult
    {
        public RomarrValidationResult()
        {
            Failures = new List<RomarrValidationFailure>();
            Errors = new List<RomarrValidationFailure>();
            Warnings = new List<RomarrValidationFailure>();
        }

        public RomarrValidationResult(ValidationResult validationResult)
            : this(validationResult.Errors)
        {
        }

        public RomarrValidationResult(IEnumerable<ValidationFailure> failures)
        {
            var errors = new List<RomarrValidationFailure>();
            var warnings = new List<RomarrValidationFailure>();

            foreach (var failureBase in failures)
            {
                if (failureBase is not RomarrValidationFailure failure)
                {
                    failure = new RomarrValidationFailure(failureBase);
                }

                if (failure.IsWarning)
                {
                    warnings.Add(failure);
                }
                else
                {
                    errors.Add(failure);
                }
            }

            Failures = errors.Concat(warnings).ToList();
            Errors = errors;
            errors.ForEach(base.Errors.Add);
            Warnings = warnings;
        }

        public IList<RomarrValidationFailure> Failures { get; private set; }
        public new IList<RomarrValidationFailure> Errors { get; private set; }
        public IList<RomarrValidationFailure> Warnings { get; private set; }

        public virtual bool HasWarnings => Warnings.Any();

        public override bool IsValid => Errors.Empty();
    }
}
