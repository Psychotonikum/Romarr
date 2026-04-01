using System.Collections.Generic;
using Romarr.Core.Messaging.Commands;
using Romarr.Core.Messaging.Events;
using Romarr.Core.Games.Commands;
using Romarr.Core.Games.Events;

namespace Romarr.Core.Games
{
    public class GameEditedService : IHandle<GameEditedEvent>
    {
        private readonly IManageCommandQueue _commandQueueManager;

        public GameEditedService(IManageCommandQueue commandQueueManager)
        {
            _commandQueueManager = commandQueueManager;
        }

        public void Handle(GameEditedEvent message)
        {
            if (message.Game.GameType != message.OldSeries.GameType)
            {
                _commandQueueManager.Push(new RefreshGameCommand(new List<int> { message.Game.Id }, false));
            }
        }
    }
}
