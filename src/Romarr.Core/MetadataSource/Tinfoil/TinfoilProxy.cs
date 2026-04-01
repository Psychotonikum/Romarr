using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Text.RegularExpressions;
using NLog;
using Romarr.Common.Http;

namespace Romarr.Core.MetadataSource.Tinfoil
{
    public interface ITinfoilProxy
    {
        List<TinfoilTitle> GetTitlesForGame(string gameTitle);
        List<TinfoilTitle> GetTitlesByTitleId(string titleId);
        List<TinfoilTitle> GetTitleDetails(string baseTitleId);
    }

    public class TinfoilTitle
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Version { get; set; }
        public string Region { get; set; }
        public string Type { get; set; } // "Base", "Update", "DLC"
        public long? Size { get; set; }
        public string ReleaseDate { get; set; }
        public string Publisher { get; set; }
    }

    public class TinfoilProxy : ITinfoilProxy
    {
        private const string TinfoilApiBase = "https://tinfoil.io/Title/ApiJson/";
        private const string TinfoilTitleBase = "https://tinfoil.io/Title/";
        private readonly IHttpClient _httpClient;
        private readonly Logger _logger;
        private Dictionary<string, TinfoilTitle> _titleCache;
        private DateTime _cacheExpiry;

        public TinfoilProxy(IHttpClient httpClient, Logger logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            _titleCache = new Dictionary<string, TinfoilTitle>();
            _cacheExpiry = DateTime.MinValue;
        }

        public List<TinfoilTitle> GetTitlesForGame(string gameTitle)
        {
            try
            {
                EnsureCacheLoaded();

                var normalizedSearch = NormalizeName(gameTitle);

                return _titleCache.Values
                    .Where(t => t.Name != null &&
                                NormalizeName(t.Name).Contains(normalizedSearch, StringComparison.OrdinalIgnoreCase))
                    .OrderBy(t => t.Type == "Base" ? 0 : t.Type == "Update" ? 1 : 2)
                    .ThenBy(t => t.ReleaseDate)
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.Warn(ex, "Failed to search tinfoil titles for: {0}", gameTitle);
                return new List<TinfoilTitle>();
            }
        }

        public List<TinfoilTitle> GetTitlesByTitleId(string titleId)
        {
            try
            {
                EnsureCacheLoaded();

                // Switch title IDs share a base: the last 3 hex digits encode type
                // Base: xxx000, Update: xxx800, DLC: xxx001-xxx7FF
                var baseTitleId = GetBaseTitleId(titleId);

                return _titleCache.Values
                    .Where(t => t.Id != null && GetBaseTitleId(t.Id) == baseTitleId)
                    .OrderBy(t => t.Type == "Base" ? 0 : t.Type == "Update" ? 1 : 2)
                    .ThenBy(t => t.ReleaseDate)
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.Warn(ex, "Failed to fetch tinfoil titles for ID: {0}", titleId);
                return new List<TinfoilTitle>();
            }
        }

        public List<TinfoilTitle> GetTitleDetails(string baseTitleId)
        {
            var results = new List<TinfoilTitle>();

            try
            {
                var request = new HttpRequest(TinfoilTitleBase + baseTitleId)
                {
                    AllowAutoRedirect = true,
                    RequestTimeout = TimeSpan.FromSeconds(15)
                };

                request.Headers.Add("User-Agent", "Romarr/1.0");

                var response = _httpClient.Get(request);

                if (response.StatusCode != HttpStatusCode.OK)
                {
                    _logger.Warn("Tinfoil title page returned status {0} for {1}", response.StatusCode, baseTitleId);
                    return results;
                }

                var html = response.Content;

                // Extract the game name from the page title
                var nameMatch = Regex.Match(html, @"<h1[^>]*>\s*(.+?)\s*</h1>", RegexOptions.Singleline);
                var gameName = nameMatch.Success ? StripHtml(nameMatch.Groups[1].Value).Trim() : "Unknown";

                // Parse patches section
                var patchesMatch = Regex.Match(html, @"<h4>Patches</h4>(.*?)</table>", RegexOptions.Singleline);
                if (patchesMatch.Success)
                {
                    var patchRows = Regex.Matches(patchesMatch.Groups[1].Value, @"<tr>\s*<td>(\d{4}-\d{2}-\d{2})</td>\s*<td>(v\d+)</td>", RegexOptions.Singleline);
                    var updateTitleId = baseTitleId[..^3] + "800";

                    foreach (Match row in patchRows)
                    {
                        results.Add(new TinfoilTitle
                        {
                            Id = updateTitleId,
                            Name = $"{gameName} {row.Groups[2].Value}",
                            Version = row.Groups[2].Value,
                            Type = "Update",
                            ReleaseDate = row.Groups[1].Value
                        });
                    }
                }

                // Parse DLC / Add-On Content section
                var dlcMatch = Regex.Match(html, @"Add-On Content.*?</table>", RegexOptions.Singleline);
                if (dlcMatch.Success)
                {
                    var dlcRows = Regex.Matches(dlcMatch.Value, @"<a href=""/Title/([^""]+)"">([^<]+)</a>.*?<td>(v\d+)</td>", RegexOptions.Singleline);

                    foreach (Match row in dlcRows)
                    {
                        results.Add(new TinfoilTitle
                        {
                            Id = row.Groups[1].Value,
                            Name = StripHtml(row.Groups[2].Value).Trim(),
                            Version = row.Groups[3].Value,
                            Type = "DLC"
                        });
                    }
                }

                _logger.Info("Fetched {0} patches and {1} DLCs from Tinfoil for {2}",
                    results.Count(r => r.Type == "Update"),
                    results.Count(r => r.Type == "DLC"),
                    baseTitleId);
            }
            catch (Exception ex)
            {
                _logger.Warn(ex, "Failed to fetch title details from Tinfoil for {0}", baseTitleId);
            }

            return results;
        }

        private void EnsureCacheLoaded()
        {
            if (_titleCache.Count > 0 && DateTime.UtcNow < _cacheExpiry)
            {
                return;
            }

            _logger.Info("Loading Tinfoil title database...");

            var request = new HttpRequest(TinfoilApiBase)
            {
                AllowAutoRedirect = true,
                RequestTimeout = TimeSpan.FromSeconds(30)
            };

            request.Headers.Add("User-Agent", "Romarr/1.0");

            var response = _httpClient.Get(request);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                _logger.Warn("Tinfoil API returned status {0}", response.StatusCode);
                return;
            }

            var newCache = new Dictionary<string, TinfoilTitle>();

            using var doc = JsonDocument.Parse(response.Content);
            var root = doc.RootElement;

            // The API returns {"data": [array of objects]} format
            if (root.ValueKind == JsonValueKind.Object &&
                root.TryGetProperty("data", out var dataArray) &&
                dataArray.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in dataArray.EnumerateArray())
                {
                    if (item.ValueKind != JsonValueKind.Object)
                    {
                        continue;
                    }

                    var titleId = GetStringProp(item, "id");
                    if (string.IsNullOrEmpty(titleId))
                    {
                        continue;
                    }

                    var rawName = GetStringProp(item, "name");

                    var title = new TinfoilTitle
                    {
                        Id = titleId,
                        Name = StripHtml(rawName),
                        Publisher = GetStringProp(item, "publisher"),
                        ReleaseDate = GetStringProp(item, "release_date"),
                        Type = ClassifyTitleType(titleId),
                    };

                    var sizeStr = GetStringProp(item, "size");
                    if (!string.IsNullOrEmpty(sizeStr))
                    {
                        title.Size = ParseSizeString(sizeStr);
                    }

                    newCache[titleId] = title;
                }
            }
            else if (root.ValueKind == JsonValueKind.Object)
            {
                // Legacy format: {titleId: {properties}}
                foreach (var prop in root.EnumerateObject())
                {
                    var titleId = prop.Name;

                    if (prop.Value.ValueKind != JsonValueKind.Object)
                    {
                        continue;
                    }

                    var title = new TinfoilTitle
                    {
                        Id = titleId,
                        Name = GetStringProp(prop.Value, "name"),
                        Version = GetStringProp(prop.Value, "version"),
                        Region = GetStringProp(prop.Value, "region"),
                        Publisher = GetStringProp(prop.Value, "publisher"),
                        ReleaseDate = GetStringProp(prop.Value, "releaseDate"),
                        Type = ClassifyTitleType(titleId),
                    };

                    if (prop.Value.TryGetProperty("size", out var sizeEl) && sizeEl.ValueKind == JsonValueKind.Number)
                    {
                        title.Size = sizeEl.GetInt64();
                    }

                    newCache[titleId] = title;
                }
            }

            _titleCache = newCache;
            _cacheExpiry = DateTime.UtcNow.AddHours(12);
            _logger.Info("Loaded {0} titles from Tinfoil database", _titleCache.Count);
        }

        private static string ClassifyTitleType(string titleId)
        {
            if (string.IsNullOrEmpty(titleId) || titleId.Length < 4)
            {
                return "Base";
            }

            var suffix = titleId[^3..].ToLowerInvariant();

            if (suffix == "000")
            {
                return "Base";
            }

            if (suffix == "800")
            {
                return "Update";
            }

            return "DLC";
        }

        private static string GetBaseTitleId(string titleId)
        {
            if (string.IsNullOrEmpty(titleId) || titleId.Length < 4)
            {
                return titleId ?? string.Empty;
            }

            return titleId[..^3].ToLowerInvariant();
        }

        private static string GetStringProp(JsonElement element, string name)
        {
            return element.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String
                ? value.GetString()
                : null;
        }

        private static string StripHtml(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return input;
            }

            return Regex.Replace(input, @"<[^>]+>", string.Empty);
        }

        private static string NormalizeName(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return string.Empty;
            }

            // Remove trademark/copyright symbols and other special characters for fuzzy matching
            return Regex.Replace(input, @"[™®©]", string.Empty).ToLowerInvariant().Trim();
        }

        private static long? ParseSizeString(string sizeStr)
        {
            var match = Regex.Match(sizeStr, @"([\d.]+)\s*(GB|MB|KB|TB)", RegexOptions.IgnoreCase);
            if (!match.Success)
            {
                return null;
            }

            if (!double.TryParse(match.Groups[1].Value, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var value))
            {
                return null;
            }

            return match.Groups[2].Value.ToUpperInvariant() switch
            {
                "TB" => (long)(value * 1099511627776),
                "GB" => (long)(value * 1073741824),
                "MB" => (long)(value * 1048576),
                "KB" => (long)(value * 1024),
                _ => null
            };
        }
    }
}
