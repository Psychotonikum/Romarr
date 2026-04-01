using Microsoft.AspNetCore.Mvc;
using Romarr.Core.Datastore.Events;
using Romarr.Core.HealthCheck;
using Romarr.Core.Messaging.Events;
using Romarr.SignalR;
using Romarr.Http;
using Romarr.Http.REST;

namespace Romarr.Api.V5.Health;

[V5ApiController]
public class HealthController : RestControllerWithSignalR<HealthResource, HealthCheck>,
                            IHandle<HealthCheckCompleteEvent>
{
    private readonly IHealthCheckService _healthCheckService;

    public HealthController(IBroadcastSignalRMessage signalRBroadcaster, IHealthCheckService healthCheckService)
        : base(signalRBroadcaster)
    {
        _healthCheckService = healthCheckService;
    }

    [NonAction]
    public override ActionResult<HealthResource> GetResourceByIdWithErrorHandler(int id)
    {
        return base.GetResourceByIdWithErrorHandler(id);
    }

    protected override HealthResource GetResourceById(int id)
    {
        throw new NotImplementedException();
    }

    [HttpGet]
    [Produces("application/json")]
    public List<HealthResource> GetHealth()
    {
        return _healthCheckService.Results().ToResource();
    }

    [NonAction]
    public void Handle(HealthCheckCompleteEvent message)
    {
        BroadcastResourceChange(ModelAction.Sync);
    }
}
