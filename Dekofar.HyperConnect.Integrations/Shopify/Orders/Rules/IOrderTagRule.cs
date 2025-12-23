using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dekofar.HyperConnect.Integrations.Shopify.Orders.Rules
{
    /// <summary>
    /// Shopify siparişi için otomatik etiket kuralı
    /// </summary>
    public interface IOrderTagRule
    {
        /// <summary>
        /// Sipariş payload'ını değerlendirir ve
        /// eklenmesi gereken etiketleri döner
        /// </summary>
        Task<IEnumerable<string>> EvaluateAsync(
            JObject order,
            CancellationToken ct);
    }
}
