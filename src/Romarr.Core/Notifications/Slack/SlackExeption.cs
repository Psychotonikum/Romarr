using System;
using Romarr.Common.Exceptions;

namespace Romarr.Core.Notifications.Slack
{
    public class SlackExeption : RomarrException
    {
        public SlackExeption(string message)
            : base(message)
        {
        }

        public SlackExeption(string message, Exception innerException, params object[] args)
            : base(message, innerException, args)
        {
        }
    }
}
