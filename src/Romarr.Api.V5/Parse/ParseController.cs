using Microsoft.AspNetCore.Mvc;
using Romarr.Common.Extensions;
using Romarr.Core.CustomFormats;
using Romarr.Core.Download.Aggregation;
using Romarr.Core.Parser;
using Romarr.Api.V5.CustomFormats;
using Romarr.Api.V5.Roms;
using Romarr.Api.V5.Game;
using Romarr.Http;

namespace Romarr.Api.V5.Parse;

[V5ApiController]
public class ParseController : Controller
{
    private readonly IParsingService _parsingService;
    private readonly IRemoteFileAggregationService _aggregationService;
    private readonly ICustomFormatCalculationService _formatCalculator;

    public ParseController(IParsingService parsingService,
                           IRemoteFileAggregationService aggregationService,
                           ICustomFormatCalculationService formatCalculator)
    {
        _parsingService = parsingService;
        _aggregationService = aggregationService;
        _formatCalculator = formatCalculator;
    }

    [HttpGet]
    [Produces("application/json")]
    public ParseResource Parse(string? title, string? path)
    {
        if (title.IsNullOrWhiteSpace())
        {
            return new ParseResource
            {
                Title = title
            };
        }

        var parsedRomInfo = path.IsNotNullOrWhiteSpace() ? Parser.ParsePath(path) : Parser.ParseTitle(title);

        if (parsedRomInfo == null)
        {
            return new ParseResource
            {
                Title = title
            };
        }

        var remoteRom = _parsingService.Map(parsedRomInfo, 0, 0, null);

        if (remoteRom != null)
        {
            _aggregationService.Augment(remoteRom);

            remoteRom.CustomFormats = _formatCalculator.ParseCustomFormat(remoteRom, 0);
            remoteRom.CustomFormatScore = remoteRom.Game?.QualityProfile?.Value.CalculateCustomFormatScore(remoteRom.CustomFormats) ?? 0;

            return new ParseResource
            {
                Title = title,
                ParsedRomInfo = remoteRom.ParsedRomInfo,
                Game = remoteRom.Game?.ToResource(),
                Roms = remoteRom.Roms.ToResource(),
                Languages = remoteRom.Languages,
                CustomFormats = remoteRom.CustomFormats?.ToResource(false),
                CustomFormatScore = remoteRom.CustomFormatScore
            };
        }
        else
        {
            return new ParseResource
            {
                Title = title,
                ParsedRomInfo = parsedRomInfo
            };
        }
    }
}
