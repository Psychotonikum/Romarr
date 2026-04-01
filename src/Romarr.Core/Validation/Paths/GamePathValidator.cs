using System.Linq;
using FluentValidation.Validators;
using Romarr.Common.Disk;
using Romarr.Common.Extensions;
using Romarr.Core.Games;

namespace Romarr.Core.Validation.Paths
{
    public class GamePathValidator : PropertyValidator
    {
        private readonly IGameService _gameService;

        public GamePathValidator(IGameService seriesService)
        {
            _gameService = seriesService;
        }

        protected override string GetDefaultMessageTemplate() => "Path '{path}' is already configured for another game";

        protected override bool IsValid(PropertyValidatorContext context)
        {
            if (context.PropertyValue == null)
            {
                return true;
            }

            context.MessageFormatter.AppendArgument("path", context.PropertyValue.ToString());

            dynamic instance = context.ParentContext.InstanceToValidate;
            var instanceId = (int)instance.Id;

            // Skip the path for this game and any invalid paths
            return !_gameService.GetAllGamePaths().Any(s => s.Key != instanceId &&
                                                                s.Value.IsPathValid(PathValidationType.CurrentOs) &&
                                                                s.Value.PathEquals(context.PropertyValue.ToString()));
        }
    }
}
