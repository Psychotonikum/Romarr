using System;
using System.Collections.Generic;
using NLog;
using Romarr.Common.Http;

namespace Romarr.Core.MetadataSource.WiiU
{
    public interface IWiiUTitleProxy
    {
        List<WiiUTitle> GetTitleDetails(string gameTitle);
    }

    public class WiiUTitle
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Version { get; set; }
        public string Region { get; set; }
        public string Type { get; set; } // "Update", "DLC"
        public string ReleaseDate { get; set; }
    }

    public class WiiUTitleProxy : IWiiUTitleProxy
    {
        private readonly IHttpClient _httpClient;
        private readonly Logger _logger;
        private Dictionary<string, List<WiiUTitle>> _titleCache;
        private DateTime _cacheExpiry;

        public WiiUTitleProxy(IHttpClient httpClient, Logger logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            _titleCache = new Dictionary<string, List<WiiUTitle>>(StringComparer.OrdinalIgnoreCase);
            _cacheExpiry = DateTime.MinValue;
        }

        public List<WiiUTitle> GetTitleDetails(string gameTitle)
        {
            // Wii U title data can be enriched from GameTDB or similar sources.
            // For now, we return empty results; IGDB already provides DLC data.
            // When a reliable Wii U update/DLC data source is identified,
            // implement the lookup here following the TinfoilProxy pattern.
            _logger.Debug("Wii U title enrichment not yet implemented for: {0}", gameTitle);
            return new List<WiiUTitle>();
        }
    }
}
