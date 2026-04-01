using Romarr.Core.Notifications;

namespace Romarr.Core.Housekeeping.Housekeepers
{
    public class FixFutureNotificationStatusTimes : FixFutureProviderStatusTimes<NotificationStatus>, IHousekeepingTask
    {
        public FixFutureNotificationStatusTimes(INotificationStatusRepository notificationStatusRepository)
            : base(notificationStatusRepository)
        {
        }
    }
}
