using System;
using NLog;
using Romarr.Common.EnvironmentInfo;
using Romarr.Core.Messaging.Events;
using Romarr.Core.ThingiProvider.Status;

namespace Romarr.Core.Notifications
{
    public interface INotificationStatusService : IProviderStatusServiceBase<NotificationStatus>
    {
    }

    public class NotificationStatusService : ProviderStatusServiceBase<INotification, NotificationStatus>, INotificationStatusService
    {
        public NotificationStatusService(INotificationStatusRepository providerStatusRepository, IEventAggregator eventAggregator, IRuntimeInfo runtimeInfo, Logger logger)
            : base(providerStatusRepository, eventAggregator, runtimeInfo, logger)
        {
            MinimumTimeSinceInitialFailure = TimeSpan.FromMinutes(5);
            MaximumEscalationLevel = 5;
        }
    }
}
