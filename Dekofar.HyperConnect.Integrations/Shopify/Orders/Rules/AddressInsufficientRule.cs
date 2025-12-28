using Dekofar.HyperConnect.Integrations.Shopify.Orders.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dekofar.HyperConnect.Integrations.Shopify.Orders.Rules
{
    public class AddressInsufficientRule : IOrderTagRule
    {
        private static readonly string[] Keywords =
        {
        "avm", "sinema", "kargo", "kargodan", "şube",
        "teslim al", "hastane"
    };

        public Task<OrderTagResult?> EvaluateAsync(JObject order, CancellationToken ct)
        {
            var address =
                order["shipping_address"]?["address1"]?.ToString()?.ToLowerInvariant();

            if (string.IsNullOrWhiteSpace(address))
                return null;

            if (address.Length < 10 ||
                Keywords.Any(k => address.Contains(k)))
            {
                return Task.FromResult<OrderTagResult?>(new OrderTagResult
                {
                    Tag = "ara1",
                    Reason = "Adres yetersiz",
                    Priority = 95,
                    Note = "Adres AVM / şube / teslim noktası içeriyor veya yetersiz"
                });
            }

            return Task.FromResult<OrderTagResult?>(null);
        }
    }

}
