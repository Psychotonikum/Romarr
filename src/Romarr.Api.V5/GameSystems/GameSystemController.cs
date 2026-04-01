using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Romarr.Core.Datastore.Events;
using Romarr.Core.Games;
using Romarr.Core.Games.Events;
using Romarr.Core.Messaging.Events;
using Romarr.SignalR;
using Romarr.Http;
using Romarr.Http.REST;
using Romarr.Http.REST.Attributes;

namespace Romarr.Api.V5.GameSystems;

[V5ApiController]
public class GameSystemController : RestControllerWithSignalR<GameSystemResource, GameSystem>,
                                    IHandle<GameSystemUpdatedEvent>
{
    private readonly IGameSystemService _gameSystemService;

    public GameSystemController(IBroadcastSignalRMessage signalRBroadcaster,
                                IGameSystemService gameSystemService)
        : base(signalRBroadcaster)
    {
        _gameSystemService = gameSystemService;

        SharedValidator.RuleFor(s => s.Name)
            .NotEmpty()
            .WithMessage("System name is required");

        SharedValidator.RuleFor(s => s.FolderName)
            .NotEmpty()
            .WithMessage("Folder name is required")
            .Matches("^[a-z0-9_-]+$")
            .WithMessage("Folder name must be lowercase alphanumeric with optional hyphens or underscores");
    }

    protected override GameSystemResource GetResourceById(int id)
    {
        return _gameSystemService.Get(id).ToResource();
    }

    [HttpGet]
    [Produces("application/json")]
    public List<GameSystemResource> GetAll()
    {
        return _gameSystemService.All().ToResource();
    }

    [RestPostById]
    [Consumes("application/json")]
    [Produces("application/json")]
    public ActionResult<GameSystemResource> Create([FromBody] GameSystemResource resource)
    {
        var model = resource.ToModel();
        var created = _gameSystemService.Add(model);

        return Created(created.Id);
    }

    [RestPutById]
    [Consumes("application/json")]
    [Produces("application/json")]
    public ActionResult<GameSystemResource> Update([FromBody] GameSystemResource resource)
    {
        var model = resource.ToModel();
        _gameSystemService.Update(model);

        return Accepted(resource.Id);
    }

    [RestDeleteById]
    public ActionResult Delete(int id)
    {
        _gameSystemService.Delete(id);

        return NoContent();
    }

    [NonAction]
    public void Handle(GameSystemUpdatedEvent message)
    {
        BroadcastResourceChange(ModelAction.Sync);
    }
}
