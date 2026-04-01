using FluentValidation.Results;

namespace Romarr.Core.Validation
{
    public class RomarrValidationFailure : ValidationFailure
    {
        public bool IsWarning { get; set; }
        public string DetailedDescription { get; set; }
        public string InfoLink { get; set; }

        public RomarrValidationFailure(string propertyName, string error)
            : base(propertyName, error)
        {
        }

        public RomarrValidationFailure(string propertyName, string error, object attemptedValue)
            : base(propertyName, error, attemptedValue)
        {
        }

        public RomarrValidationFailure(ValidationFailure validationFailure)
            : base(validationFailure.PropertyName, validationFailure.ErrorMessage, validationFailure.AttemptedValue)
        {
            CustomState = validationFailure.CustomState;
            var state = validationFailure.CustomState as RomarrValidationState;

            IsWarning = state is { IsWarning: true };
        }
    }
}
