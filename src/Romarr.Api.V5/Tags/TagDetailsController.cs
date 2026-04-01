using Microsoft.AspNetCore.Mvc;
using Romarr.Core.Tags;
using Romarr.Http;
using Romarr.Http.REST;

namespace Romarr.Api.V5.Tags;

[V5ApiController("tag/detail")]
public class TagDetailsController : RestController<TagDetailsResource>
{
    private readonly ITagService _tagService;

    public TagDetailsController(ITagService tagService)
    {
        _tagService = tagService;
    }

    protected override TagDetailsResource GetResourceById(int id)
    {
        return _tagService.Details(id).ToResource();
    }

    [HttpGet]
    [Produces("application/json")]
    public List<TagDetailsResource> GetAll()
    {
        return _tagService.Details().ToResource();
    }
}
