using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Dekofar.HyperConnect.Integrations.Shopify.Utils
{
    public static class ShopifyHmacValidator
    {
        public static bool IsValid(string secret, string body, string shopifyHmac)
        {
            var keyBytes = Encoding.UTF8.GetBytes(secret);
            using var hmac = new HMACSHA256(keyBytes);
            var hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(body));
            var hash = Convert.ToBase64String(hashBytes);
            return hash == shopifyHmac;
        }
    }
}
