using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dekofar.HyperConnect.Integrations.Shopify.Orders.Rules
{
    /// <summary>
    /// Siparişin kargo adresi tam mı kontrol eder
    /// </summary>
    public class AddressCheckRule : IOrderTagRule
    {
        public Task<IEnumerable<string>> EvaluateAsync(
            JObject order,
            CancellationToken ct)
        {
            var address = order["shipping_address"];

            if (address == null)
            {
                return Task.FromResult<IEnumerable<string>>(new[]
                {
                    "ADRES_EKSIK",
                    "MANUEL_KONTROL"
                });
            }

            bool hasAllFields =
                !string.IsNullOrWhiteSpace(address["address1"]?.ToString()) &&
                !string.IsNullOrWhiteSpace(address["city"]?.ToString()) &&
                !string.IsNullOrWhiteSpace(address["zip"]?.ToString()) &&
                !string.IsNullOrWhiteSpace(address["country_code"]?.ToString());

            if (!hasAllFields)
            {
                return Task.FromResult<IEnumerable<string>>(new[]
                {
                    "ADRES_EKSIK",
                    "MANUEL_KONTROL"
                });
            }

            return Task.FromResult(Enumerable.Empty<string>());
        }
    }
}
