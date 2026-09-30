using System.Xml;
using Microsoft.AspNetCore.Mvc;
using Romarr.Core.MetadataSource.RomDatabase;
using Romarr.Http;

namespace Romarr.Api.V5.RomDatabase
{
    [V5ApiController]
    public class RomDatabaseController : Controller
    {
        private readonly IRomDatabaseService _romDatabaseService;

        public RomDatabaseController(IRomDatabaseService romDatabaseService)
        {
            _romDatabaseService = romDatabaseService;
        }

        [HttpGet("systems")]
        [Produces("application/json")]
        public List<RomDatabaseSystem> GetSystems()
        {
            return _romDatabaseService.GetAvailableSystems();
        }

        [HttpGet("systems/{systemId}/status")]
        [Produces("application/json")]
        public RomDatabaseStatus GetSystemStatus(string systemId)
        {
            return _romDatabaseService.GetSystemStatus(systemId);
        }

        [HttpPost("systems/{systemId}/import")]
        [RequestFormLimits(MultipartBodyLengthLimit = 25000000)]
        public IActionResult ImportDatabase(string systemId)
        {
            if (Request.Form.Files.Count != 1 || Request.Form.Files[0].Length == 0 || Request.Form.Files[0].Length > 25000000)
            {
                return BadRequest(new { message = "Provide one DAT file up to 25 MB." });
            }

            try
            {
                using var stream = Request.Form.Files[0].OpenReadStream();
                _romDatabaseService.ImportDatabase(systemId, stream);
                return Ok(_romDatabaseService.GetSystemStatus(systemId));
            }
            catch (Exception ex) when (ex is InvalidDataException or XmlException or ArgumentException)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("lookup/hash")]
        [Produces("application/json")]
        public ActionResult<RomDatabaseEntry> LookupByHash([FromQuery] string systemId, [FromQuery] string hash)
        {
            var result = _romDatabaseService.LookupByHash(systemId, hash);

            if (result == null)
            {
                return NotFound();
            }

            return result;
        }

        [HttpGet("lookup/name")]
        [Produces("application/json")]
        public ActionResult<RomDatabaseEntry> LookupByName([FromQuery] string systemId, [FromQuery] string filename)
        {
            var result = _romDatabaseService.LookupByName(systemId, filename);

            if (result == null)
            {
                return NotFound();
            }

            return result;
        }
    }
}
