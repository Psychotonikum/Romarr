using Microsoft.AspNetCore.Mvc;
using Romarr.Common.Extensions;
using Romarr.Core.Datastore.Events;
using Romarr.Core.Download.Pending;
using Romarr.Core.Messaging.Events;
using Romarr.Core.Queue;
using Romarr.SignalR;
using Romarr.Http;
using Romarr.Http.REST;

namespace Romarr.Api.V5.Queue
{
    [V5ApiController("queue/details")]
    public class QueueDetailsController : RestControllerWithSignalR<QueueResource, Romarr.Core.Queue.Queue>,
                               IHandle<QueueUpdatedEvent>, IHandle<PendingReleasesUpdatedEvent>
    {
        private readonly IQueueService _queueService;
        private readonly IPendingReleaseService _pendingReleaseService;

        public QueueDetailsController(IBroadcastSignalRMessage broadcastSignalRMessage, IQueueService queueService, IPendingReleaseService pendingReleaseService)
            : base(broadcastSignalRMessage)
        {
            _queueService = queueService;
            _pendingReleaseService = pendingReleaseService;
        }

        [NonAction]
        public override ActionResult<QueueResource> GetResourceByIdWithErrorHandler(int id)
        {
            return base.GetResourceByIdWithErrorHandler(id);
        }

        protected override QueueResource GetResourceById(int id)
        {
            throw new NotImplementedException();
        }

        [HttpGet]
        [Produces("application/json")]
        public List<QueueResource> GetQueue(int? gameId, [FromQuery]List<int> romIds, [FromQuery] QueueSubresource[]? includeSubresources = null)
        {
            var queue = _queueService.GetQueue();
            var pending = _pendingReleaseService.GetPendingQueue();
            var fullQueue = queue.Concat(pending);
            var includeSeries = includeSubresources.Contains(QueueSubresource.Game);
            var includeGameFiles = includeSubresources.Contains(QueueSubresource.Roms);

            if (gameId.HasValue)
            {
                return fullQueue.Where(q => q.Game?.Id == gameId).ToResource(includeSeries, includeGameFiles);
            }

            if (romIds.Any())
            {
                return fullQueue.Where(q => q.Roms.Any() &&
                                            romIds.IntersectBy(e => e, q.Roms, e => e.Id, null).Any())
                    .ToResource(includeSeries, includeGameFiles);
            }

            return fullQueue.ToResource(includeSeries, includeGameFiles);
        }

        [NonAction]
        public void Handle(QueueUpdatedEvent message)
        {
            BroadcastResourceChange(ModelAction.Sync);
        }

        [NonAction]
        public void Handle(PendingReleasesUpdatedEvent message)
        {
            BroadcastResourceChange(ModelAction.Sync);
        }
    }
}
