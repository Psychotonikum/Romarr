namespace Romarr.Core.Validation
{
    public class RomarrValidationState
    {
        public static RomarrValidationState Warning = new RomarrValidationState { IsWarning = true };

        public bool IsWarning { get; set; }
    }
}
