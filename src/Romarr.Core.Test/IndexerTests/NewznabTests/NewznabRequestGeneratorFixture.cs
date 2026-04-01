using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Romarr.Core.Indexers;
using Romarr.Core.Indexers.Newznab;
using Romarr.Core.IndexerSearch.Definitions;
using Romarr.Core.Test.Framework;

namespace Romarr.Core.Test.IndexerTests.NewznabTests
{
    public class NewznabRequestGeneratorFixture : CoreTest<NewznabRequestGenerator>
    {
        private SingleGameFileSearchCriteria _singleGameFileSearchCriteria;
        private PlatformSearchCriteria _platformSearchCriteria;
        private NewznabCapabilities _capabilities;

        [SetUp]
        public void SetUp()
        {
            Subject.Definition = new IndexerDefinition
            {
                Name = "Newznab"
            };

            Subject.Settings = new NewznabSettings()
            {
                BaseUrl = "http://127.0.0.1:1234/",
                Categories = new[] { 1, 2 },
                ApiKey = "abcd",
            };

            _singleGameFileSearchCriteria = new SingleGameFileSearchCriteria
            {
                Game = new Games.Game { MobyGamesId = 10, IgdbId = 20, RawgId = 30, ImdbId = "t40", TmdbId = 50 },
                SceneTitles = new List<string> { "Monkey Island" },
                PlatformNumber = 1,
                FileNumber = 2
            };

            _platformSearchCriteria = new PlatformSearchCriteria
            {
                Game = new Games.Game { MobyGamesId = 10, IgdbId = 20, RawgId = 30, ImdbId = "t40", TmdbId = 50 },
                SceneTitles = new List<string> { "Monkey Island" },
                PlatformNumber = 1,
            };

            _capabilities = new NewznabCapabilities();

            Mocker.GetMock<INewznabCapabilitiesProvider>()
                .Setup(v => v.GetCapabilities(It.IsAny<NewznabSettings>()))
                .Returns(_capabilities);
        }

        [Test]
        public void should_use_all_categories_for_feed()
        {
            var results = Subject.GetRecentRequests();

            results.GetAllTiers().Should().HaveCount(1);

            var page = results.GetAllTiers().First().First();

            page.Url.Query.Should().Contain("&cat=1,2&");
        }

        [Test]
        public void should_not_have_duplicate_categories()
        {
            Subject.Settings.Categories = new[] { 1, 2, 3 };

            var results = Subject.GetRecentRequests();

            results.GetAllTiers().Should().HaveCount(1);

            var page = results.GetAllTiers().First().First();

            page.Url.FullUri.Should().Contain("&cat=1,2,3&");
        }

        [Test]
        public void should_not_search_by_rid_if_not_supported()
        {
            _capabilities.SupportedTvSearchParameters = new[] { "q", "platform", "ep" };

            var results = Subject.GetSearchRequests(_singleGameFileSearchCriteria);

            results.GetAllTiers().Should().HaveCount(2);

            var page = results.GetAllTiers().First().First();

            page.Url.Query.Should().NotContain("rid=10");
            page.Url.Query.Should().Contain("q=Monkey");
        }

        [Test]
        public void should_search_by_rid_if_supported()
        {
            var results = Subject.GetSearchRequests(_singleGameFileSearchCriteria);
            results.GetTier(0).Should().HaveCount(1);

            var page = results.GetAllTiers().First().First();

            page.Url.Query.Should().Contain("rid=10");
        }

        [Test]
        public void should_not_search_by_igdbid_if_not_supported()
        {
            _capabilities.SupportedTvSearchParameters = new[] { "q", "platform", "ep" };

            var results = Subject.GetSearchRequests(_singleGameFileSearchCriteria);
            results.GetTier(0).Should().HaveCount(1);

            var page = results.GetAllTiers().First().First();

            page.Url.Query.Should().NotContain("rid=10");
            page.Url.Query.Should().Contain("q=Monkey");
        }

        [Test]
        public void should_search_by_igdbid_if_supported()
        {
            _capabilities.SupportedTvSearchParameters = new[] { "q", "igdbid", "platform", "ep" };

            var results = Subject.GetSearchRequests(_singleGameFileSearchCriteria);
            results.GetTier(0).Should().HaveCount(1);

            var page = results.GetAllTiers().First().First();

            page.Url.Query.Should().Contain("igdbid=20");
        }

        [Test]
        public void should_search_by_tvmaze_if_supported()
        {
            _capabilities.SupportedTvSearchParameters = new[] { "q", "tvmazeid", "platform", "ep" };

            var results = Subject.GetSearchRequests(_singleGameFileSearchCriteria);
            results.GetTier(0).Should().HaveCount(1);

            var page = results.GetAllTiers().First().First();

            page.Url.Query.Should().Contain("tvmazeid=30");
        }

        [Test]
        public void should_search_by_imdbid_if_supported()
        {
            _capabilities.SupportedTvSearchParameters = new[] { "q", "imdbid", "platform", "ep" };

            var results = Subject.GetSearchRequests(_singleGameFileSearchCriteria);
            results.GetTier(0).Should().HaveCount(1);

            var page = results.GetAllTiers().First().First();

            page.Url.Query.Should().Contain("imdbid=t40");
        }

        [Test]
        public void should_search_by_tmdb_if_supported()
        {
            _capabilities.SupportedTvSearchParameters = new[] { "q", "tmdbid", "platform", "ep" };

            var results = Subject.GetSearchRequests(_singleGameFileSearchCriteria);
            results.GetTier(0).Should().HaveCount(1);

            var page = results.GetAllTiers().First().First();

            page.Url.Query.Should().Contain("tmdbid=50");
        }

        [Test]
        public void should_prefer_search_by_igdbid_if_rid_supported()
        {
            _capabilities.SupportedTvSearchParameters = new[] { "q", "igdbid", "rid", "platform", "ep" };

            var results = Subject.GetSearchRequests(_singleGameFileSearchCriteria);
            results.GetTier(0).Should().HaveCount(1);

            var page = results.GetAllTiers().First().First();

            page.Url.Query.Should().Contain("igdbid=20");
            page.Url.Query.Should().NotContain("rid=10");
        }

        [Test]
        public void should_use_aggregrated_id_search_if_supported()
        {
            _capabilities.SupportedTvSearchParameters = new[] { "q", "igdbid", "rid", "platform", "ep" };
            _capabilities.SupportsAggregateIdSearch = true;

            var results = Subject.GetSearchRequests(_singleGameFileSearchCriteria);
            results.GetTier(0).Should().HaveCount(1);

            var page = results.GetTier(0).First().First();

            page.Url.Query.Should().Contain("igdbid=20");
            page.Url.Query.Should().Contain("rid=10");
        }

        [Test]
        public void should_not_use_aggregrated_id_search_if_no_ids_supported()
        {
            _capabilities.SupportedTvSearchParameters = new[] { "q", "platform", "ep" };
            _capabilities.SupportsAggregateIdSearch = true; // Turns true if indexer supplies supportedParams.

            var results = Subject.GetSearchRequests(_singleGameFileSearchCriteria);
            results.Tiers.Should().Be(2);
            results.GetTier(0).Should().HaveCount(1);

            var page = results.GetTier(0).First().First();

            page.Url.Query.Should().Contain("q=");
        }

        [Test]
        public void should_not_use_aggregrated_id_search_if_no_ids_are_known()
        {
            _capabilities.SupportedTvSearchParameters = new[] { "q", "rid", "platform", "ep" };
            _capabilities.SupportsAggregateIdSearch = true; // Turns true if indexer supplies supportedParams.

            _singleGameFileSearchCriteria.Game.MobyGamesId = 0;

            var results = Subject.GetSearchRequests(_singleGameFileSearchCriteria);

            var page = results.GetTier(0).First().First();

            page.Url.Query.Should().Contain("q=");
        }

        [Test]
        public void should_fallback_to_title()
        {
            _capabilities.SupportedTvSearchParameters = new[] { "q", "title", "igdbid", "rid", "platform", "ep" };
            _capabilities.SupportsAggregateIdSearch = true;

            var results = Subject.GetSearchRequests(_singleGameFileSearchCriteria);
            results.Tiers.Should().Be(3);

            var pageTier2 = results.GetTier(1).First().First();

            pageTier2.Url.Query.Should().NotContain("igdbid=20");
            pageTier2.Url.Query.Should().NotContain("rid=10");
            pageTier2.Url.Query.Should().NotContain("q=");
            pageTier2.Url.Query.Should().Contain("title=Monkey%20Island");
        }

        [Test]
        public void should_url_encode_title()
        {
            _capabilities.SupportedTvSearchParameters = new[] { "q", "title", "igdbid", "rid", "platform", "ep" };
            _capabilities.SupportsAggregateIdSearch = true;

            _singleGameFileSearchCriteria.SceneTitles[0] = "Elith & Little";

            var results = Subject.GetSearchRequests(_singleGameFileSearchCriteria);
            results.Tiers.Should().Be(3);

            var pageTier2 = results.GetTier(1).First().First();

            pageTier2.Url.Query.Should().NotContain("igdbid=20");
            pageTier2.Url.Query.Should().NotContain("rid=10");
            pageTier2.Url.Query.Should().NotContain("q=");
            pageTier2.Url.Query.Should().Contain("title=Elith%20%26%20Little");
        }

        [Test]
        public void should_fallback_to_q()
        {
            _capabilities.SupportedTvSearchParameters = new[] { "q", "igdbid", "rid", "platform", "ep" };
            _capabilities.SupportsAggregateIdSearch = true;

            var results = Subject.GetSearchRequests(_singleGameFileSearchCriteria);
            results.Tiers.Should().Be(3);

            var pageTier2 = results.GetTier(1).First().First();

            pageTier2.Url.Query.Should().NotContain("igdbid=20");
            pageTier2.Url.Query.Should().NotContain("rid=10");
            pageTier2.Url.Query.Should().Contain("q=");
        }

        [Test]
        public void should_encode_raw_title()
        {
            _capabilities.SupportedTvSearchParameters = new[] { "q", "platform", "ep" };
            _capabilities.TvTextSearchEngine = "raw";
            _singleGameFileSearchCriteria.SceneTitles[0] = "Edith & Little";

            var results = Subject.GetSearchRequests(_singleGameFileSearchCriteria);
            results.Tiers.Should().Be(2);

            var pageTier = results.GetTier(0).First().First();

            pageTier.Url.Query.Should().Contain("q=Edith%20%26%20Little");
            pageTier.Url.Query.Should().NotContain(" & ");
            pageTier.Url.Query.Should().Contain("%26");
        }

        [Test]
        public void should_use_clean_title_and_encode()
        {
            _capabilities.SupportedTvSearchParameters = new[] { "q", "platform", "ep" };
            _capabilities.TvTextSearchEngine = "sphinx";
            _singleGameFileSearchCriteria.SceneTitles[0] = "Edith & Little";

            var results = Subject.GetSearchRequests(_singleGameFileSearchCriteria);
            results.Tiers.Should().Be(2);

            var pageTier = results.GetTier(0).First().First();

            pageTier.Url.Query.Should().Contain("q=Edith%20and%20Little");
            pageTier.Url.Query.Should().Contain("and");
            pageTier.Url.Query.Should().NotContain(" & ");
            pageTier.Url.Query.Should().NotContain("%26");
        }

        [Test]
        public void should_allow_platform_search_even_if_gameFile_search_is_not_allowed()
        {
            _capabilities.SupportedTvSearchParameters = new[] { "q", "igdbid", "platform" };

            var results = Subject.GetSearchRequests(_platformSearchCriteria);
            results.GetTier(0).Should().HaveCount(1);

            var page = results.GetAllTiers().First().First();

            page.Url.Query.Should().Contain("igdbid=20");
        }

        [Test]
        public void should_search_even_without_platform_param()
        {
            _capabilities.SupportedTvSearchParameters = new[] { "q", "igdbid" };

            var results = Subject.GetSearchRequests(_platformSearchCriteria);
            results.GetTier(0).Should().HaveCount(1);

            var page = results.GetTier(0).First().First();
            page.Url.Query.Should().Contain("igdbid=20");
        }
    }
}
