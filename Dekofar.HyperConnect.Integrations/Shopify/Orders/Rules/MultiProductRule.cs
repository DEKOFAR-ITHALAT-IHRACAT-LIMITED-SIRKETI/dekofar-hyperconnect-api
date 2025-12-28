using Dekofar.HyperConnect.Integrations.Shopify.Orders.Models;
using Newtonsoft.Json.Linq;

namespace Dekofar.HyperConnect.Integrations.Shopify.Orders.Rules;

public class MultiProductRule : IOrderTagRule
{
    public Task<OrderTagResult?> EvaluateAsync(JObject order, CancellationToken ct)
    {
        var distinctProducts =
            order["line_items"]?
                .Select(li => li["product_id"]?.ToString())
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Distinct()
                .Count() ?? 0;

        if (distinctProducts > 1)
        {
            var r = new OrderTagResult
            {
                Tag = "ara1",
                Priority = 80
            };
            r.Reasons.Add("Birden fazla ürün çeşidi");
            r.Notes.Add("Siparişte birden fazla ürün var");

            return Task.FromResult<OrderTagResult?>(r);
        }

        return Task.FromResult<OrderTagResult?>(null);
    }
}
