using System;
using System.Security.Cryptography;
using System.Text;

namespace Dekofar.HyperConnect.Integrations.Shopify.Utils
{
    public static class ShopifyHmacValidator
    {
        /// <summary>
        /// Shopify Webhook HMAC doğrulaması
        /// </summary>
        /// <param name="requestBody">RAW request body (JSON string)</param>
        /// <param name="shopifyHmacHeader">X-Shopify-Hmac-Sha256 header değeri</param>
        /// <param name="webhookSecret">Shopify Webhook Secret</param>
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

            // Güvenli string karşılaştırma
            return SlowEquals(calculatedHmac, shopifyHmacHeader);
        }

        private static bool SlowEquals(string a, string b)
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
