using System.Collections.Generic;
using System.Linq;
using Romarr.Core.Messaging.Commands;
using Romarr.Core.Messaging.Events;
using Romarr.Core.Games.Commands;
using Romarr.Core.Games.Events;

namespace Romarr.Core.Games
{
    public class GameAddedHandler : IHandle<GameAddedEvent>,
                                      IHandle<GameImportedEvent>
    {
        private readonly IManageCommandQueue _commandQueueManager;

        public GameAddedHandler(IManageCommandQueue commandQueueManager)
        {
            _commandQueueManager = commandQueueManager;
        }

        public void Handle(GameAddedEvent message)
        {
            _commandQueueManager.Push(new RefreshGameCommand(new List<int> { message.Game.Id }, true));
        }

        public void Handle(GameImportedEvent message)
        {
            _commandQueueManager.PushMany(message.GameIds.Select(s => new RefreshGameCommand(new List<int> { s }, true)).ToList());
        }
    }
}
