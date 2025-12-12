using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dekofar.HyperConnect.Integrations.Shopify.Models.Shopify
{
    public class Fulfillment
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("tracking_number")]
        public string? TrackingNumber { get; set; }

        [JsonProperty("tracking_numbers")]
        public List<string>? TrackingNumbers { get; set; }

        [JsonProperty("tracking_company")]
        public string? TrackingCompany { get; set; }

        [JsonProperty("status")]
        public string? Status { get; set; }

        [JsonProperty("created_at")]
        public string? CreatedAt { get; set; }
    }
}
