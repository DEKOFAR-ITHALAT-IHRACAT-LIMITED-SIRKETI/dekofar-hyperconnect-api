using Dekofar.HyperConnect.Integrations.Shopify.Orders.Models;
using Newtonsoft.Json.Linq;

namespace Dekofar.HyperConnect.Integrations.Shopify.Orders.Rules;

public class AddressInsufficientRule : IOrderTagRule
{
    private static readonly string[] Keywords =
    {
        "avm",
        "sinema",
        "kargo",
        "kargodan",
        "şube",
        "sube",
        "teslim al",
        "hastane"
    };

    public Task<OrderTagResult?> EvaluateAsync(
        JObject order,
        CancellationToken ct)
    {
        var address =
            order["shipping_address"]?["address1"]?
                .ToString()
                ?.ToLowerInvariant();

        if (string.IsNullOrWhiteSpace(address))
            return Task.FromResult<OrderTagResult?>(null);

        var isTooShort = address.Length < 10;
        var hasKeyword = Keywords.Any(k => address.Contains(k));

        if (!isTooShort && !hasKeyword)
            return Task.FromResult<OrderTagResult?>(null);

        var result = new OrderTagResult
        {
            Tag = "ara1",
            Priority = 95,
            ReasonCode = "ADDRESS_INSUFFICIENT"
        };

        if (isTooShort)
            result.Notes.Add("Adres uzunluğu 10 karakterden kısa");

        if (hasKeyword)
            result.Notes.Add("Adres AVM / şube / teslim noktası içeriyor");

        return Task.FromResult<OrderTagResult?>(result);
    }
}
