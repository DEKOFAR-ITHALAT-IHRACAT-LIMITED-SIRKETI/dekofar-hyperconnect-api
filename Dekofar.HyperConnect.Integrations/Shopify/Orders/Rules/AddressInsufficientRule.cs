using Dekofar.HyperConnect.Integrations.Shopify.Orders.Models;
using Newtonsoft.Json.Linq;

namespace Dekofar.HyperConnect.Integrations.Shopify.Orders.Rules;

public class AddressInsufficientRule : IOrderTagRule
{
    private static readonly string[] Keywords =
    {
        "avm", "sinema", "kargo", "kargodan",
        "şube", "teslim al", "hastane"
    };

    public Task<OrderTagResult?> EvaluateAsync(JObject order, CancellationToken ct)
    {
        var address =
            order["shipping_address"]?["address1"]?
                .ToString()?.ToLowerInvariant();

        if (string.IsNullOrWhiteSpace(address))
            return Task.FromResult<OrderTagResult?>(null);

        if (address.Length < 10 || Keywords.Any(k => address.Contains(k)))
        {
            var r = new OrderTagResult
            {
                Tag = "ara1",
                Priority = 95
            };

            r.Reasons.Add("Adres yetersiz");
            r.Notes.Add("Adres AVM / şube / teslim noktası içeriyor veya çok kısa");

            return Task.FromResult<OrderTagResult?>(r);
        }

        return Task.FromResult<OrderTagResult?>(null);
    }
}
