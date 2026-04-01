using System;
using System.Collections.Generic;
using System.Net;
using FluentValidation.Results;
using Newtonsoft.Json;
using NLog;
using Romarr.Common.Extensions;
using Romarr.Common.Http;
using Romarr.Core.Localization;

namespace Romarr.Core.ImportLists.Romarr
{
    public interface IRomarrV3Proxy
    {
        List<RomarrSeries> GetGame(RomarrSettings settings);
        List<RomarrProfile> GetQualityProfiles(RomarrSettings settings);
        List<RomarrProfile> GetLanguageProfiles(RomarrSettings settings);
        List<RomarrRootFolder> GetRootFolders(RomarrSettings settings);
        List<RomarrTag> GetTags(RomarrSettings settings);
        ValidationFailure Test(RomarrSettings settings);
    }

    public class RomarrV3Proxy : IRomarrV3Proxy
    {
        private readonly IHttpClient _httpClient;
        private readonly Logger _logger;
        private readonly ILocalizationService _localizationService;

        public RomarrV3Proxy(IHttpClient httpClient, ILocalizationService localizationService, Logger logger)
        {
            _httpClient = httpClient;
            _localizationService = localizationService;
            _logger = logger;
        }

        public List<RomarrSeries> GetGame(RomarrSettings settings)
        {
            return Execute<RomarrSeries>("/api/v3/game", settings);
        }

        public List<RomarrProfile> GetQualityProfiles(RomarrSettings settings)
        {
            return Execute<RomarrProfile>("/api/v3/qualityprofile", settings);
        }

        public List<RomarrProfile> GetLanguageProfiles(RomarrSettings settings)
        {
            return Execute<RomarrProfile>("/api/v3/languageprofile", settings);
        }

        public List<RomarrRootFolder> GetRootFolders(RomarrSettings settings)
        {
            return Execute<RomarrRootFolder>("api/v3/rootfolder", settings);
        }

        public List<RomarrTag> GetTags(RomarrSettings settings)
        {
            return Execute<RomarrTag>("/api/v3/tag", settings);
        }

        public ValidationFailure Test(RomarrSettings settings)
        {
            try
            {
                GetGame(settings);
            }
            catch (HttpException ex)
            {
                if (ex.Response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    _logger.Error(ex, "API Key is invalid");
                    return new ValidationFailure("ApiKey", _localizationService.GetLocalizedString("ImportListsValidationInvalidApiKey"));
                }

                if (ex.Response.HasHttpRedirect)
                {
                    _logger.Error(ex, "Romarr returned redirect and is invalid");
                    return new ValidationFailure("BaseUrl", _localizationService.GetLocalizedString("ImportListsRomarrValidationInvalidUrl"));
                }

                _logger.Error(ex, "Unable to connect to import list.");
                return new ValidationFailure(string.Empty, _localizationService.GetLocalizedString("ImportListsValidationUnableToConnectException", new Dictionary<string, object> { { "exceptionMessage", ex.Message } }));
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Unable to connect to import list.");
                return new ValidationFailure(string.Empty, _localizationService.GetLocalizedString("ImportListsValidationUnableToConnectException", new Dictionary<string, object> { { "exceptionMessage", ex.Message } }));
            }

            return null;
        }

        private List<TResource> Execute<TResource>(string resource, RomarrSettings settings)
        {
            if (settings.BaseUrl.IsNullOrWhiteSpace() || settings.ApiKey.IsNullOrWhiteSpace())
            {
                return new List<TResource>();
            }

            var baseUrl = settings.BaseUrl.TrimEnd('/');

            var request = new HttpRequestBuilder(baseUrl).Resource(resource)
                .Accept(HttpAccept.Json)
                .SetHeader("X-Api-Key", settings.ApiKey)
                .Build();

            var response = _httpClient.Get(request);

            if ((int)response.StatusCode >= 300)
            {
                throw new HttpException(response);
            }

            var results = JsonConvert.DeserializeObject<List<TResource>>(response.Content);

            return results;
        }
    }
}
