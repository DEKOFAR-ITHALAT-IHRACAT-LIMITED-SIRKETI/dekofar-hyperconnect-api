using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Collections.Generic;

namespace Dekofar.HyperConnect.Integrations.Shopify.Common
{
    public static class ShopifyHmacValidator
    {
        public static bool IsValid(
            IDictionary<string, string> query,
            string clientSecret)
        {
            if (!query.ContainsKey("hmac"))
                return false;

            var receivedHmac = query["hmac"];

            var sortedQuery = query
                .Where(x => x.Key != "hmac")
                .OrderBy(x => x.Key)
                .Select(x => $"{x.Key}={x.Value}")
                .ToArray();

            var message = string.Join("&", sortedQuery);

            using var hmacsha256 =
                new HMACSHA256(Encoding.UTF8.GetBytes(clientSecret));

            var hashBytes =
                hmacsha256.ComputeHash(Encoding.UTF8.GetBytes(message));

            var calculatedHmac =
                BitConverter.ToString(hashBytes)
                    .Replace("-", "")
                    .ToLowerInvariant();

            return calculatedHmac == receivedHmac;
        }
    }
}
