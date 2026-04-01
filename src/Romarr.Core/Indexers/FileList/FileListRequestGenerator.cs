using System;
using System.Collections.Generic;
using System.Linq;
using Romarr.Common.Extensions;
using Romarr.Common.Http;
using Romarr.Core.DataAugmentation.Scene;
using Romarr.Core.IndexerSearch.Definitions;

namespace Romarr.Core.Indexers.FileList
{
    public class FileListRequestGenerator : IIndexerRequestGenerator
    {
        public FileListSettings Settings { get; set; }

        public virtual IndexerPageableRequestChain GetRecentRequests()
        {
            var pageableRequests = new IndexerPageableRequestChain();

            pageableRequests.Add(GetRequest("latest-torrents", Settings.Categories.Concat(Settings.AnimeCategories), ""));

            return pageableRequests;
        }

        public IndexerPageableRequestChain GetSearchRequests(SingleGameFileSearchCriteria searchCriteria)
        {
            var pageableRequests = new IndexerPageableRequestChain();

            if (searchCriteria.SearchMode.HasFlag(SearchMode.SearchID) || searchCriteria.SearchMode == SearchMode.Default)
            {
                AddImdbRequests(pageableRequests, searchCriteria, "search-torrents", Settings.Categories, $"&platform={searchCriteria.PlatformNumber}&rom={searchCriteria.FileNumber}");
            }

            if (searchCriteria.SearchMode.HasFlag(SearchMode.SearchTitle))
            {
                AddNameRequests(pageableRequests, searchCriteria, "search-torrents", Settings.Categories, $"&platform={searchCriteria.PlatformNumber}&rom={searchCriteria.FileNumber}");
            }

            pageableRequests.AddTier();

            if (searchCriteria.SearchMode == SearchMode.Default)
            {
                AddNameRequests(pageableRequests, searchCriteria, "search-torrents", Settings.Categories, $"&platform={searchCriteria.PlatformNumber}&rom={searchCriteria.FileNumber}");
            }

            return pageableRequests;
        }

        public IndexerPageableRequestChain GetSearchRequests(PlatformSearchCriteria searchCriteria)
        {
            var pageableRequests = new IndexerPageableRequestChain();

            if (searchCriteria.SearchMode.HasFlag(SearchMode.SearchID) || searchCriteria.SearchMode == SearchMode.Default)
            {
                AddImdbRequests(pageableRequests, searchCriteria, "search-torrents", Settings.Categories, $"&platform={searchCriteria.PlatformNumber}");
            }

            if (searchCriteria.SearchMode.HasFlag(SearchMode.SearchTitle))
            {
                AddNameRequests(pageableRequests, searchCriteria, "search-torrents", Settings.Categories, $"&platform={searchCriteria.PlatformNumber}");
            }

            pageableRequests.AddTier();

            if (searchCriteria.SearchMode == SearchMode.Default)
            {
                AddNameRequests(pageableRequests, searchCriteria, "search-torrents", Settings.Categories, $"&platform={searchCriteria.PlatformNumber}");
            }

            return pageableRequests;
        }

        public IndexerPageableRequestChain GetSearchRequests(SpecialGameFileSearchCriteria searchCriteria)
        {
            return new IndexerPageableRequestChain();
        }

        private void AddImdbRequests(IndexerPageableRequestChain chain, SearchCriteriaBase searchCriteria, string searchType, IEnumerable<int> categories, string parameters)
        {
            if (searchCriteria.Game.ImdbId.IsNotNullOrWhiteSpace())
            {
                chain.Add(GetRequest(searchType, categories, string.Format("&type=imdb&query={0}{1}", searchCriteria.Game.ImdbId, parameters)));
            }
        }

        private void AddNameRequests(IndexerPageableRequestChain chain, SearchCriteriaBase searchCriteria, string searchType, IEnumerable<int> categories, string parameters)
        {
            foreach (var sceneTitle in searchCriteria.SceneTitles)
            {
                chain.Add(GetRequest(searchType, categories, string.Format("&type=name&query={0}{1}", Uri.EscapeDataString(sceneTitle.Trim()), parameters)));
            }
        }

        private IEnumerable<IndexerRequest> GetRequest(string searchType, IEnumerable<int> categories, string parameters)
        {
            if (categories.Empty())
            {
                yield break;
            }

            var categoriesQuery = string.Join(",", categories.Distinct());

            var baseUrl = string.Format("{0}/api.php?action={1}&category={2}{3}", Settings.BaseUrl.TrimEnd('/'), searchType, categoriesQuery, parameters);

            var request = new IndexerRequest(baseUrl, HttpAccept.Json);
            request.HttpRequest.Credentials = new BasicNetworkCredential(Settings.Username.Trim(), Settings.Passkey.Trim());

            yield return request;
        }
    }
}
