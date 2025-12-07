using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Dekofar.HyperConnect.Integrations.Meta.Interfaces;
using Dekofar.HyperConnect.Integrations.Meta.Models;

namespace Dekofar.HyperConnect.Integrations.Meta.Services
{
    public class FacebookMarketingApiClient : IFacebookMarketingApiClient
    {
        private readonly HttpClient _httpClient;

        public FacebookMarketingApiClient(HttpClient httpClient)
        {
            // BaseAddress Program.cs'te verilecek
            _httpClient = httpClient;
        }

        public async Task<IReadOnlyList<FacebookAdDto>> GetActiveAdsAsync(
            string adAccountId,
            string accessToken,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(adAccountId))
                throw new ArgumentException("adAccountId is required", nameof(adAccountId));

            if (string.IsNullOrWhiteSpace(accessToken))
                throw new ArgumentException("accessToken is required", nameof(accessToken));

            var requestUri =
                $"act_{adAccountId}/ads" +
                "?fields=id,name,effective_status,creative{effective_object_story_id}" +
                "&effective_status=['ACTIVE']" +
                $"&access_token={Uri.EscapeDataString(accessToken)}";

            var response = await _httpClient.GetFromJsonAsync<FacebookAdsResponse>(
                requestUri,
                cancellationToken);

            var data = response?.Data ?? new List<FacebookAdData>();

            return data.Select(a => new FacebookAdDto
            {
                Id = a.Id,
                Name = a.Name,
                EffectiveStatus = a.EffectiveStatus,
                PostId = a.Creative?.EffectiveObjectStoryId
            }).ToList();
        }
    }
}
