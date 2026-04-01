using System;
using System.Net;
using System.Text.Json;
using System.Text.RegularExpressions;
using NLog;
using Romarr.Common.Http;

namespace Romarr.Core.MetadataSource.Metacritic
{
    public interface IMetacriticProxy
    {
        decimal? GetMetacriticScore(string gameTitle, int? year);
    }

    /// <summary>
    /// Metacritic data provider using the chrismichaelps/metacritic URL patterns for HTML
    /// scraping (primary) with the Fandom API as fallback. Ported from the unofficial-metacritic
    /// npm package (https://github.com/chrismichaelps/metacritic).
    /// </summary>
    public class MetacriticProxy : IMetacriticProxy
    {
        // chrismichaelps/metacritic URL patterns
        private const string MetacriticBase = "https://www.metacritic.com";

        // Fandom API fallback
        private const string FandomApiBase = "https://internal-prod.apigee.fandom.net/v2";

        private static readonly string[] Platforms = { "pc", "ps5", "ps4", "xbox-series-x", "xboxone", "switch" };

        private readonly IHttpClient _httpClient;
        private readonly Logger _logger;

        public MetacriticProxy(IHttpClient httpClient, Logger logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public decimal? GetMetacriticScore(string gameTitle, int? year)
        {
            // Primary: scrape the game's Metacritic page for JSON-LD aggregateRating
            try
            {
                var score = ScrapeGameScore(gameTitle);

                if (score.HasValue)
                {
                    return score;
                }
            }
            catch (Exception ex)
            {
                _logger.Debug(ex, "Metacritic HTML score scraping failed for {0}", gameTitle);
            }

            // Fallback: Fandom API search
            try
            {
                var searchTitle = Uri.EscapeDataString(gameTitle);
                var request = new HttpRequest($"{FandomApiBase}/search/game/{searchTitle}")
                {
                    AllowAutoRedirect = true,
                    RequestTimeout = TimeSpan.FromSeconds(10)
                };

                request.Headers.Add("User-Agent", "Romarr/1.0");

                var response = _httpClient.Get(request);

                if (response.StatusCode != HttpStatusCode.OK)
                {
                    return null;
                }

                return ParseFandomSearchScore(response.Content, gameTitle);
            }
            catch (Exception ex)
            {
                _logger.Debug(ex, "Failed to get Metacritic score for {0}", gameTitle);
                return null;
            }
        }

        // --- chrismichaelps-style HTML scraping (ported from unofficial-metacritic) ---

        private decimal? ScrapeGameScore(string gameTitle)
        {
            // Convert game title to Metacritic slug format
            var slug = Regex.Replace(gameTitle.ToLowerInvariant(), @"[^a-z0-9]+", "-").Trim('-');

            // Try common platforms
            foreach (var platform in Platforms)
            {
                try
                {
                    var url = $"{MetacriticBase}/game/{slug}/{platform}";
                    var html = FetchPage(url);

                    if (string.IsNullOrWhiteSpace(html))
                    {
                        continue;
                    }

                    // Extract JSON-LD structured data (chrismichaelps approach: script type="application/ld+json")
                    var jsonLdMatch = Regex.Match(html, @"<script[^>]*type=""application/ld\+json""[^>]*>(.*?)</script>", RegexOptions.Singleline);

                    if (jsonLdMatch.Success)
                    {
                        using var doc = JsonDocument.Parse(jsonLdMatch.Groups[1].Value);
                        var root = doc.RootElement;

                        if (root.TryGetProperty("aggregateRating", out var rating) &&
                            rating.TryGetProperty("ratingValue", out var ratingValue))
                        {
                            var rawScore = ratingValue.ValueKind == JsonValueKind.Number
                                ? ratingValue.GetDecimal()
                                : decimal.TryParse(ratingValue.GetString(), out var parsed) ? parsed : (decimal?)null;

                            if (rawScore.HasValue)
                            {
                                // Metacritic scores are 0-100, normalize to 0-10
                                return rawScore.Value / 10m;
                            }
                        }
                    }

                    // Fallback: extract metascore from HTML (chrismichaelps css selector approach)
                    var scoreMatch = Regex.Match(html, @"class=""metascore_w[^""]*""\s*>(\d+)<");

                    if (scoreMatch.Success && int.TryParse(scoreMatch.Groups[1].Value, out var metascore))
                    {
                        return metascore / 10m;
                    }
                }
                catch (Exception ex)
                {
                    _logger.Debug(ex, "Failed to scrape Metacritic score from {0}/{1}", platform, slug);
                }
            }

            return null;
        }

        private string FetchPage(string url)
        {
            var request = new HttpRequest(url)
            {
                AllowAutoRedirect = true,
                RequestTimeout = TimeSpan.FromSeconds(15)
            };

            request.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");

            var response = _httpClient.Get(request);

            return response.StatusCode == HttpStatusCode.OK ? response.Content : null;
        }

        // --- Fandom API fallback ---

        private decimal? ParseFandomSearchScore(string content, string gameTitle)
        {
            using var doc = JsonDocument.Parse(content);
            var results = doc.RootElement;

            if (results.TryGetProperty("data", out var data) && data.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in data.EnumerateArray())
                {
                    if (item.TryGetProperty("title", out var title) &&
                        title.GetString()?.Equals(gameTitle, StringComparison.OrdinalIgnoreCase) == true)
                    {
                        if (item.TryGetProperty("score", out var score) && score.ValueKind == JsonValueKind.Number)
                        {
                            return score.GetDecimal();
                        }
                    }
                }

                if (data.GetArrayLength() > 0)
                {
                    var first = data[0];

                    if (first.TryGetProperty("score", out var firstScore) && firstScore.ValueKind == JsonValueKind.Number)
                    {
                        return firstScore.GetDecimal();
                    }
                }
            }

            return null;
        }
    }
}
