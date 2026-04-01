using FluentValidation;
using FluentValidation.Results;
using Romarr.Core.Validation.Paths;

namespace Romarr.Core.Games
{
    public interface IAddGameValidator
    {
        ValidationResult Validate(Game instance);
    }

    public class AddGameValidator : AbstractValidator<Game>, IAddGameValidator
    {
        public AddGameValidator(RootFolderValidator rootFolderValidator,
                                  GamePathValidator seriesPathValidator,
                                  GameAncestorValidator seriesAncestorValidator,
                                  GameTitleSlugValidator gameTitleSlugValidator)
        {
            RuleFor(c => c.Path).Cascade(CascadeMode.Stop)
                .IsValidPath()
                                .SetValidator(rootFolderValidator)
                                .SetValidator(seriesPathValidator)
                                .SetValidator(seriesAncestorValidator);

            RuleFor(c => c.TitleSlug).SetValidator(gameTitleSlugValidator);
        }
    }
}
