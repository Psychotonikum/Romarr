using System.Collections.Generic;
using System.Linq;
using Romarr.Core.Languages;
using Romarr.Core.Games;

namespace Romarr.Core.Notifications.Webhook
{
    public class WebhookSeries
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string TitleSlug { get; set; }
        public string Path { get; set; }
        public int IgdbId { get; set; }
        public int RawgId { get; set; }
        public int TmdbId { get; set; }
        public string ImdbId { get; set; }
        public HashSet<int> MalIds { get; set; }
        public HashSet<int> AniListIds { get; set; }
        public GameTypes Type { get; set; }
        public int Year { get; set; }
        public List<string> Genres { get; set; }
        public List<WebhookImage> Images { get; set; }
        public List<string> Tags { get; set; }
        public Language OriginalLanguage { get; set; }
        public string OriginalCountry { get; set; }

        public WebhookSeries()
        {
        }

        public WebhookSeries(Game game, List<string> tags)
        {
            Id = game.Id;
            Title = game.Title;
            TitleSlug = game.TitleSlug;
            Path = game.Path;
            IgdbId = game.IgdbId;
            RawgId = game.RawgId;
            TmdbId = game.TmdbId;
            ImdbId = game.ImdbId;
            MalIds = game.MalIds;
            AniListIds = game.AniListIds;
            Type = game.GameType;
            Year = game.Year;
            Genres = game.Genres;
            Images = game.Images.Select(i => new WebhookImage(i)).ToList();
            Tags = tags;
            OriginalLanguage = game.OriginalLanguage;
            OriginalCountry = game.OriginalCountry;
        }
    }
}
