using System.Collections.Generic;
using System.Text.Json.Serialization;
using Romarr.Common.Extensions;
using Romarr.Core.Messaging.Commands;

namespace Romarr.Core.Games.Commands
{
    public class RefreshGameCommand : Command
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public int GameId
        {
            get => 0;
            set
            {
                if (GameIds.Empty())
                {
                    GameIds.Add(value);
                }
            }
        }

        public List<int> GameIds { get; set; }
        public bool IsNewSeries { get; set; }

        public RefreshGameCommand()
        {
            GameIds = new List<int>();
        }

        public RefreshGameCommand(List<int> gameIds, bool isNewSeries = false)
        {
            GameIds = gameIds;
            IsNewSeries = isNewSeries;
        }

        public override bool SendUpdatesToClient => true;

        public override bool UpdateScheduledTask => GameIds.Empty();

        public override bool IsLongRunning => true;

        public override string CompletionMessage => "Completed";
    }
}
