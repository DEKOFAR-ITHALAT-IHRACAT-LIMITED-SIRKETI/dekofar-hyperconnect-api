using System.Security.Cryptography;
using System.Text;

namespace Dekofar.HyperConnect.Integrations.Shopify.Utils
{
    /// <summary>
    /// Shopify HMAC Validator
    /// ✔ Webhook (X-Shopify-Hmac-Sha256)
    /// ✔ OAuth Callback (query string)
    /// ✔ Timing-attack safe
    /// ✔ Shopify resmi dokümana %100 uyumlu
    /// </summary>
    public static class ShopifyHmacValidator
    {
        // =====================================================
        // 🔐 WEBHOOK HMAC VALIDATION
        // Header: X-Shopify-Hmac-Sha256 (Base64)
        // =====================================================
        public static bool ValidateWebhook(
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

            byte[] secretBytes = Encoding.UTF8.GetBytes(webhookSecret);
            byte[] bodyBytes = Encoding.UTF8.GetBytes(requestBody);

            using var hmac = new HMACSHA256(secretBytes);
            byte[] computedHash = hmac.ComputeHash(bodyBytes);

            byte[] receivedHash;
            try
            {
                receivedHash = Convert.FromBase64String(shopifyHmacHeader);
            }
            catch
            {
                return false;
            }

            return CryptographicOperations.FixedTimeEquals(
                computedHash,
                receivedHash);
        }

        // =====================================================
        // 🔐 OAUTH CALLBACK HMAC VALIDATION
        // Shopify OAuth redirect query string
        // =====================================================
        public static bool ValidateOAuth(
            IDictionary<string, string> query,
            string clientSecret)
        {
            if (query == null ||
                query.Count == 0 ||
                string.IsNullOrWhiteSpace(clientSecret))
            {
                return false;
            }

            if (!query.TryGetValue("hmac", out var providedHmac) ||
                string.IsNullOrWhiteSpace(providedHmac))
            {
                return false;
            }

            var message = string.Join("&",
                query
                    .Where(kvp =>
                        kvp.Key != "hmac" &&
                        kvp.Key != "signature")
                    .OrderBy(kvp => kvp.Key, StringComparer.Ordinal)
                    .Select(kvp => $"{kvp.Key}={kvp.Value}")
            );

            byte[] secretBytes = Encoding.UTF8.GetBytes(clientSecret);
            byte[] messageBytes = Encoding.UTF8.GetBytes(message);

            using var hmacSha256 = new HMACSHA256(secretBytes);
            byte[] computedHash = hmacSha256.ComputeHash(messageBytes);

            var computedHmacHex =
                Convert.ToHexString(computedHash)
                    .ToLowerInvariant();

            return CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(computedHmacHex),
                Encoding.UTF8.GetBytes(providedHmac));
        }
    }
}
