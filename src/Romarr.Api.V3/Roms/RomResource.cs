using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Romarr.Core.MediaCover;
using Romarr.Core.Games;
using Romarr.Api.V3.RomFiles;
using Romarr.Api.V3.Game;
using Romarr.Http.REST;
using Swashbuckle.AspNetCore.Annotations;

namespace Romarr.Api.V3.Roms
{
    public class RomResource : RestResource
    {
        public int GameId { get; set; }
        public int IgdbId { get; set; }
        public int RomFileId { get; set; }
        public int PlatformNumber { get; set; }
        public int RomNumber { get; set; }
        public string Title { get; set; }
        public string AirDate { get; set; }
        public DateTime? AirDateUtc { get; set; }
        public DateTime? LastSearchTime { get; set; }
        public int Runtime { get; set; }
        public string FinaleType { get; set; }
        public string RomType { get; set; }
        public string Overview { get; set; }
        public RomFileResource RomFile { get; set; }
        public bool HasFile { get; set; }
        public bool Monitored { get; set; }
        public int? AbsoluteFileNumber { get; set; }
        public int? SceneAbsoluteFileNumber { get; set; }
        public int? SceneFileNumber { get; set; }
        public int? ScenePlatformNumber { get; set; }
        public bool UnverifiedSceneNumbering { get; set; }
        public GameResource Game { get; set; }
        public List<MediaCover> Images { get; set; }

        // Hiding this so people don't think its usable (only used to set the initial state)
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        [SwaggerIgnore]
        public bool Grabbed { get; set; }
    }

    public static class RomResourceMapper
    {
        private static readonly Regex TypePrefixRegex = new Regex(@"^\[(Update|DLC|Base)\]\s*", RegexOptions.Compiled);

        public static RomResource ToResource(this Rom model)
        {
            if (model == null)
            {
                return null;
            }

            var title = model.Title;
            if (!string.IsNullOrEmpty(title))
            {
                title = TypePrefixRegex.Replace(title, string.Empty);
            }

            var airDateUtc = model.AirDateUtc;
            if (!airDateUtc.HasValue && !string.IsNullOrEmpty(model.AirDate) &&
                DateTime.TryParse(model.AirDate, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var parsed))
            {
                airDateUtc = parsed.ToUniversalTime();
            }

            return new RomResource
            {
                Id = model.Id,

                GameId = model.GameId,
                IgdbId = model.IgdbId,
                RomFileId = model.RomFileId,
                PlatformNumber = model.PlatformNumber,
                RomNumber = model.FileNumber,
                Title = title,
                AirDate = model.AirDate,
                AirDateUtc = airDateUtc,
                Runtime = model.Runtime,
                FinaleType = model.FinaleType,
                RomType = model.RomType.ToString().ToLowerInvariant(),
                Overview = model.Overview,
                LastSearchTime = model.LastSearchTime,

                // RomFile

                HasFile = model.HasFile,
                Monitored = model.Monitored,
                AbsoluteFileNumber = model.AbsoluteFileNumber,
                SceneAbsoluteFileNumber = model.SceneAbsoluteFileNumber,
                SceneFileNumber = model.SceneFileNumber,
                ScenePlatformNumber = model.ScenePlatformNumber,
                UnverifiedSceneNumbering = model.UnverifiedSceneNumbering,

                // Game = model.Game.MapToResource(),
            };
        }

        public static List<RomResource> ToResource(this IEnumerable<Rom> models)
        {
            if (models == null)
            {
                return null;
            }

            return models.Select(ToResource).ToList();
        }
    }
}
