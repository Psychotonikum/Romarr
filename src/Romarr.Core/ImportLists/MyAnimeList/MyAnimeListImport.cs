using System;
using System.Collections.Generic;
using System.Net.Http;
using NLog;
using Romarr.Common.Cloud;
using Romarr.Common.Http;
using Romarr.Core.Configuration;
using Romarr.Core.Localization;
using Romarr.Core.Parser;
using Romarr.Core.Validation;

namespace Romarr.Core.ImportLists.MyAnimeList
{
    public class MyAnimeListImport : HttpImportListBase<MyAnimeListSettings>
    {
        public const string OAuthPath = "oauth/myanimelist/authorize";
        public const string RedirectUriPath = "oauth/myanimelist/auth";
        public const string RenewUriPath = "oauth/myanimelist/renew";

        public override string Name => "MyAnimeList";
        public override ImportListType ListType => ImportListType.Other;
        public override TimeSpan MinRefreshInterval => TimeSpan.FromHours(6);

        private readonly IImportListRepository _importListRepository;
        private readonly IHttpRequestBuilderFactory _requestBuilder;

        // This constructor the first thing that is called when romarr creates a button
        public MyAnimeListImport(IImportListRepository netImportRepository, IHttpClient httpClient, IImportListStatusService importListStatusService, IConfigService configService, IParsingService parsingService, ILocalizationService localizationService, IRomarrCloudRequestBuilder requestBuilder, Logger logger)
            : base(httpClient, importListStatusService, configService, parsingService, localizationService, logger)
        {
            _importListRepository = netImportRepository;
            _requestBuilder = requestBuilder.Services;
        }

        public override ImportListFetchResult Fetch()
        {
            if (Settings.Expires < DateTime.UtcNow.AddMinutes(5))
            {
                RefreshToken();
            }

            return FetchItems(g => g.GetListItems());
        }

        // MAL OAuth info: https://myanimelist.net/blog.php?eid=835707
        // The whole process is handled through Romarr's services.
        public override object RequestAction(string action, IDictionary<string, string> query)
        {
            if (action == "startOAuth")
            {
                var request = _requestBuilder.Create()
                    .Resource(OAuthPath)
                    .AddQueryParam("state", query["callbackUrl"])
                    .AddQueryParam("redirect_uri", _requestBuilder.Create().Resource(RedirectUriPath).Build().Url.ToString())
                    .Build();

                return new
                {
                    OauthUrl = request.Url.ToString()
                };
            }
            else if (action == "getOAuthToken")
            {
                return new
                {
                    accessToken = query["access_token"],
                    expires = DateTime.UtcNow.AddSeconds(int.Parse(query["expires_in"])),
                    refreshToken = query["refresh_token"]
                };
            }

            return new { };
        }

        public override IParseImportListResponse GetParser()
        {
            return new MyAnimeListParser(Settings);
        }

        public override IImportListRequestGenerator GetRequestGenerator()
        {
            return new MyAnimeListRequestGenerator()
            {
                Settings = Settings,
            };
        }

        private void RefreshToken()
        {
            _logger.Trace("Refreshing Token");

            Settings.Validate().Filter("RefreshToken").ThrowOnError();

            var httpReq = _requestBuilder.Create()
                .Resource(RenewUriPath)
                .AddQueryParam("refresh_token", Settings.RefreshToken)
                .Build();
            try
            {
                var httpResp = _httpClient.Get<MyAnimeListAuthToken>(httpReq);

                if (httpResp?.Resource != null)
                {
                    var token = httpResp.Resource;
                    Settings.AccessToken = token.AccessToken;
                    Settings.Expires = DateTime.UtcNow.AddSeconds(token.ExpiresIn);
                    Settings.RefreshToken = token.RefreshToken ?? Settings.RefreshToken;

                    if (Definition.Id > 0)
                    {
                        _importListRepository.UpdateSettings((ImportListDefinition)Definition);
                    }
                }
            }
            catch (HttpRequestException)
            {
                _logger.Error("Error trying to refresh MAL access token.");
            }
        }
    }
}
