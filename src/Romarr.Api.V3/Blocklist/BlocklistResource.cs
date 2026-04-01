using System;
using System.Collections.Generic;
using Romarr.Core.CustomFormats;
using Romarr.Core.Indexers;
using Romarr.Core.Languages;
using Romarr.Core.Qualities;
using Romarr.Api.V3.CustomFormats;
using Romarr.Api.V3.Game;
using Romarr.Http.REST;

namespace Romarr.Api.V3.Blocklist
{
    public class BlocklistResource : RestResource
    {
        public int GameId { get; set; }
        public List<int> RomIds { get; set; }
        public string SourceTitle { get; set; }
        public List<Language> Languages { get; set; }
        public QualityModel Quality { get; set; }
        public List<CustomFormatResource> CustomFormats { get; set; }
        public DateTime Date { get; set; }
        public DownloadProtocol Protocol { get; set; }
        public string Indexer { get; set; }
        public string Message { get; set; }

        public GameResource Game { get; set; }
    }

    public static class BlocklistResourceMapper
    {
        public static BlocklistResource MapToResource(this Romarr.Core.Blocklisting.Blocklist model, ICustomFormatCalculationService formatCalculator)
        {
            if (model == null)
            {
                return null;
            }

            return new BlocklistResource
            {
                Id = model.Id,

                GameId = model.GameId,
                RomIds = model.RomIds,
                SourceTitle = model.SourceTitle,
                Languages = model.Languages,
                Quality = model.Quality,
                CustomFormats = formatCalculator.ParseCustomFormat(model, model.Game).ToResource(false),
                Date = model.Date,
                Protocol = model.Protocol,
                Indexer = model.Indexer,
                Message = model.Message,

                Game = model.Game.ToResource()
            };
        }
    }
}
