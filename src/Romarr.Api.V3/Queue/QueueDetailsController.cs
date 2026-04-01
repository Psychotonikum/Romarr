using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Romarr.Core.Datastore.Events;
using Romarr.Core.Download.Pending;
using Romarr.Core.Messaging.Events;
using Romarr.Core.Queue;
using Romarr.SignalR;
using Romarr.Http;
using Romarr.Http.REST;

#pragma warning disable CS0612
namespace Romarr.Api.V3.Queue
{
    [V3ApiController("queue/details")]
    public class QueueDetailsController : RestControllerWithSignalR<QueueResource, Romarr.Core.Queue.Queue>,
                               IHandle<ObsoleteQueueUpdatedEvent>, IHandle<PendingReleasesUpdatedEvent>
    {
        private readonly IObsoleteQueueService _queueService;
        private readonly IPendingReleaseService _pendingReleaseService;

        public QueueDetailsController(IBroadcastSignalRMessage broadcastSignalRMessage, IObsoleteQueueService queueService, IPendingReleaseService pendingReleaseService)
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
        public List<QueueResource> GetQueue(int? gameId, [FromQuery]List<int> romIds, bool includeSeries = false, bool includeGameFile = false)
        {
            var queue = _queueService.GetQueue();
            var pending = _pendingReleaseService.GetPendingQueue();
            var fullQueue = queue.Concat(pending);

            if (gameId.HasValue)
            {
                return fullQueue.Where(q => q.Game?.Id == gameId).ToResource(includeSeries, includeGameFile);
            }

            if (romIds.Any())
            {
                return fullQueue.Where(q => q.Rom != null && romIds.Contains(q.Rom.Id)).ToResource(includeSeries, includeGameFile);
            }

            return fullQueue.ToResource(includeSeries, includeGameFile);
        }

        [NonAction]
        public void Handle(ObsoleteQueueUpdatedEvent message)
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
#pragma warning restore CS0612
