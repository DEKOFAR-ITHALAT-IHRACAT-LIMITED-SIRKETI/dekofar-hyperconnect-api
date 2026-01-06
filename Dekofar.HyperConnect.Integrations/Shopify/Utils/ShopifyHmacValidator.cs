using System.Security.Cryptography;
using System.Text;

namespace Dekofar.HyperConnect.Integrations.Shopify.Utils
{
    public static class ShopifyHmacValidator
    {
        // =====================================================
        // 🔐 WEBHOOK HMAC (X-Shopify-Hmac-Sha256)
        // =====================================================
        public static bool Validate(
            string requestBody,
            string shopifyHmacHeader,
            string webhookSecret)
        {
            if (string.IsNullOrWhiteSpace(requestBody) ||
                string.IsNullOrWhiteSpace(shopifyHmacHeader) ||
                string.IsNullOrWhiteSpace(webhookSecret))
            {
                return false;
            }

            var secretBytes = Encoding.UTF8.GetBytes(webhookSecret);
            var bodyBytes = Encoding.UTF8.GetBytes(requestBody);

            using var hmac = new HMACSHA256(secretBytes);
            var hashBytes = hmac.ComputeHash(bodyBytes);

            var calculatedHmac =
                Convert.ToBase64String(hashBytes);

            return FixedTimeEquals(
                calculatedHmac,
                shopifyHmacHeader);
        }

        // =====================================================
        // 🔐 OAUTH HMAC (QUERY STRING)
        // Shopify OAuth callback için
        // =====================================================
        public static bool Validate(
            IDictionary<string, string> query,
            string clientSecret)
        {
            if (query == null || query.Count == 0)
                return false;

            if (!query.TryGetValue("hmac", out var hmac))
                return false;

            var sorted = query
                .Where(x => x.Key != "hmac" && x.Key != "signature")
                .OrderBy(x => x.Key)
                .Select(x => $"{x.Key}={x.Value}");

            var message =
                string.Join("&", sorted);

            var secretBytes = Encoding.UTF8.GetBytes(clientSecret);

            using var hmacSha256 = new HMACSHA256(secretBytes);
            var hashBytes = hmacSha256.ComputeHash(
                Encoding.UTF8.GetBytes(message));

            var calculatedHmac =
                Convert.ToHexString(hashBytes).ToLowerInvariant();

            return FixedTimeEquals(
                calculatedHmac,
                hmac);
        }

        // =====================================================
        // ⏱️ TIMING ATTACK SAFE COMPARE
        // =====================================================
        private static bool FixedTimeEquals(string a, string b)
        {
            if (a.Length != b.Length)
                return false;

            var diff = 0;
            for (var i = 0; i < a.Length; i++)
            {
                diff |= a[i] ^ b[i];
            }

            return diff == 0;
        }
    }
}
