using System;
using Romarr.Core.Datastore;
using Romarr.Core.Messaging.Commands;

namespace Romarr.Core.Jobs
{
    public class ScheduledTask : ModelBase
    {
        public string TypeName { get; set; }
        public int Interval { get; set; }
        public DateTime LastExecution { get; set; }
        public CommandPriority Priority { get; set; }
        public DateTime LastStartTime { get; set; }

        public ScheduledTask()
        {
            Priority = CommandPriority.Low;
        }
    }
}
