using System.Linq;
using FluentValidation.Validators;
using Romarr.Common.Extensions;
using Romarr.Core.Games;

namespace Romarr.Core.Validation.Paths
{
    public class GameAncestorValidator : PropertyValidator
    {
        private readonly IGameService _gameService;

        public GameAncestorValidator(IGameService seriesService)
        {
            _gameService = seriesService;
        }

        protected override string GetDefaultMessageTemplate() => "Path '{path}' is an ancestor of an existing game";

        protected override bool IsValid(PropertyValidatorContext context)
        {
            if (context.PropertyValue == null)
            {
                return true;
            }

            context.MessageFormatter.AppendArgument("path", context.PropertyValue.ToString());

            return !_gameService.GetAllGamePaths().Any(s => context.PropertyValue.ToString().IsParentPath(s.Value));
        }
    }
}
