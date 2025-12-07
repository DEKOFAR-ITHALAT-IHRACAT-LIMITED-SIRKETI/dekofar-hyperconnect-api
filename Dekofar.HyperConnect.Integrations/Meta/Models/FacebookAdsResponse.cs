using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Dekofar.HyperConnect.Integrations.Meta.Models
{
    // Graph API response modeli
    public class FacebookAdsResponse
    {
        [JsonPropertyName("data")]
        public List<FacebookAdData> Data { get; set; } = new();
    }

    public class FacebookAdData
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = default!;

        [JsonPropertyName("name")]
        public string Name { get; set; } = default!;

        [JsonPropertyName("effective_status")]
        public string EffectiveStatus { get; set; } = default!;

        [JsonPropertyName("creative")]
        public FacebookAdCreative? Creative { get; set; }
    }

    public class FacebookAdCreative
    {
        [JsonPropertyName("effective_object_story_id")]
        public string? EffectiveObjectStoryId { get; set; }
    }
}
