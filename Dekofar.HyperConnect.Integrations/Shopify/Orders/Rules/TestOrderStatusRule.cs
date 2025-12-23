using Newtonsoft.Json.Linq;

namespace Dekofar.HyperConnect.Integrations.Shopify.Orders.Rules
{
    /// <summary>
    /// SADECE:
    /// - Açık
    /// - Gönderilmemiş
    /// - Ödeme beklemede
    /// siparişlere TEST etiketi ekler
    /// </summary>
    public class TestOrderStatusRule : IOrderTagRule
    {
        public Task<IEnumerable<string>> EvaluateAsync(
            JObject order,
            CancellationToken ct)
        {
            var tags = new List<string>();

            var financialStatus =
                order["financial_status"]?.ToString();

            var fulfillmentStatus =
                order["fulfillment_status"]?.ToString();

            var closedAt =
                order["closed_at"]?.ToString();

            var isOpen = string.IsNullOrEmpty(closedAt);
            var isUnfulfilled =
                string.IsNullOrEmpty(fulfillmentStatus) ||
                fulfillmentStatus == "unfulfilled";
            var isPaymentPending =
                financialStatus == "pending";

            if (isOpen && isUnfulfilled && isPaymentPending)
            {
                tags.Add("TEST");
            }

            return Task.FromResult<IEnumerable<string>>(tags);
        }
    }
}
