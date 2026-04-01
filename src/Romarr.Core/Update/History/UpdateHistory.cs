using System;
using Romarr.Core.Datastore;

namespace Romarr.Core.Update.History
{
    public class UpdateHistory : ModelBase
    {
        public DateTime Date { get; set; }
        public Version Version { get; set; }
        public UpdateHistoryEventType EventType { get; set; }
    }
}
