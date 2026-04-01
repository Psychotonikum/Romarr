using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Newtonsoft.Json;
using Romarr.Common.Http;
using Romarr.Common.Serializer;
using Romarr.Core.IndexerSearch.Definitions;

namespace Romarr.Core.Indexers.HDBits
{
    public class HDBitsRequestGenerator : IIndexerRequestGenerator
    {
        public HDBitsSettings Settings { get; set; }

        public virtual IndexerPageableRequestChain GetRecentRequests()
        {
            var pageableRequests = new IndexerPageableRequestChain();

            pageableRequests.Add(GetRequest(new TorrentQuery()));

            return pageableRequests;
        }

        public virtual IndexerPageableRequestChain GetSearchRequests(SpecialGameFileSearchCriteria searchCriteria)
        {
            return new IndexerPageableRequestChain();
        }

        public virtual IndexerPageableRequestChain GetSearchRequests(PlatformSearchCriteria searchCriteria)
        {
            var pageableRequests = new IndexerPageableRequestChain();
            var queryBase = new TorrentQuery();

            if (TryAddSearchParameters(queryBase, searchCriteria))
            {
                foreach (var platformNumber in searchCriteria.Roms.Select(e => e.PlatformNumber).Distinct())
                {
                    var query = queryBase.Clone();

                    query.IgdbInfo.Platform = platformNumber;

                    pageableRequests.Add(GetRequest(query));
                }
            }

            return pageableRequests;
        }

        public virtual IndexerPageableRequestChain GetSearchRequests(SingleGameFileSearchCriteria searchCriteria)
        {
            var pageableRequests = new IndexerPageableRequestChain();
            var queryBase = new TorrentQuery();

            if (TryAddSearchParameters(queryBase, searchCriteria))
            {
                foreach (var rom in searchCriteria.Roms)
                {
                    var query = queryBase.Clone();

                    query.IgdbInfo.Platform = rom.PlatformNumber;
                    query.IgdbInfo.Rom = rom.FileNumber;

                    pageableRequests.Add(GetRequest(query));
                }
            }

            return pageableRequests;
        }

        private bool TryAddSearchParameters(TorrentQuery query, SearchCriteriaBase searchCriteria)
        {
            if (searchCriteria.Game.IgdbId != 0)
            {
                query.IgdbInfo ??= new IgdbInfo();
                query.IgdbInfo.Id = searchCriteria.Game.IgdbId;

                return true;
            }

            return false;
        }

        private IEnumerable<IndexerRequest> GetRequest(TorrentQuery query)
        {
            var request = new HttpRequestBuilder(Settings.BaseUrl)
                .Resource("/api/torrents")
                .Build();

            request.Method = HttpMethod.Post;
            const string appJson = "application/json";
            request.Headers.Accept = appJson;
            request.Headers.ContentType = appJson;

            query.Username = Settings.Username;
            query.Passkey = Settings.ApiKey;

            query.Category = Settings.Categories.ToArray();
            query.Codec = Settings.Codecs.ToArray();
            query.Medium = Settings.Mediums.ToArray();

            query.Limit = 100;

            request.SetContent(query.ToJson());
            request.ContentSummary = query.ToJson(Formatting.None);

            yield return new IndexerRequest(request);
        }
    }
}
