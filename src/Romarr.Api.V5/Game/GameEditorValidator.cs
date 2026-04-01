using FluentValidation;
using Romarr.Common.Extensions;
using Romarr.Core.Validation;
using Romarr.Core.Validation.Paths;

namespace Romarr.Api.V5.Game;

public class GameEditorValidator : AbstractValidator<Romarr.Core.Games.Game>
{
    public GameEditorValidator(RootFolderExistsValidator rootFolderExistsValidator, QualityProfileExistsValidator qualityProfileExistsValidator)
    {
        RuleFor(s => s.RootFolderPath).Cascade(CascadeMode.Stop)
            .IsValidPath()
            .SetValidator(rootFolderExistsValidator)
            .When(s => s.RootFolderPath.IsNotNullOrWhiteSpace());

        RuleFor(c => c.QualityProfileId).Cascade(CascadeMode.Stop)
            .ValidId()
            .SetValidator(qualityProfileExistsValidator);
    }
}
