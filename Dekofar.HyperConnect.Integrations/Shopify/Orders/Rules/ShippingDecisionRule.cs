using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;
using Dekofar.HyperConnect.Integrations.Shopify.Orders.Models;

namespace Dekofar.HyperConnect.Integrations.Shopify.Orders.Rules;

public class ShippingDecisionRule : IOrderTagRule
{
    // Gerçek "köy" tespiti (Ortaköy yakalanmaz)
    private static readonly Regex VillageRegex =
        new(@"\bköy\b|\bköyü\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public Task<OrderTagResult?> EvaluateAsync(JObject order, CancellationToken ct)
    {
        var addressObj = order["shipping_address"];
        if (addressObj == null)
            return Task.FromResult<OrderTagResult?>(null);

        var address =
            addressObj["address1"]?.ToString()?.ToLowerInvariant() ?? "";

        var city =
            addressObj["city"]?.ToString()?.ToLowerInvariant() ?? "";

        // 🟡 PTT → SADECE gerçek köy + İstanbul dışı
        if (VillageRegex.IsMatch(address) &&
            !city.Contains("istanbul"))
        {
            return Task.FromResult<OrderTagResult?>(new OrderTagResult
            {
                Tag = "ptt",
                Reason = "Adres gerçek köy ve İstanbul dışı"
            });
        }

        // 🟢 DHL → VARSAYILAN
        return Task.FromResult<OrderTagResult?>(new OrderTagResult
        {
            Tag = "dhl",
            Reason = "Varsayılan DHL (şehir içi / temiz adres)"
        });
    }
}
