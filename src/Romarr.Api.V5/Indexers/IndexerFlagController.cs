using Microsoft.AspNetCore.Mvc;
using Romarr.Core.Parser.Model;
using Romarr.Http;

namespace Romarr.Api.V5.Indexers;

[V5ApiController]
public class IndexerFlagController : Controller
{
    [HttpGet]
    public List<IndexerFlagResource> GetAll()
    {
        return Enum.GetValues(typeof(IndexerFlags)).Cast<IndexerFlags>().Select(f => new IndexerFlagResource
        {
            Id = (int)f,
            Name = f.ToString()
        }).ToList();
    }
}
