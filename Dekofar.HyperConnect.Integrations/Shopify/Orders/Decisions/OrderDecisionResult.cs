using System.Collections.Generic;

namespace Dekofar.HyperConnect.Integrations.Shopify.Orders.Decisions
{
    public class OrderDecisionResult
    {
        /// <summary>
        /// Sipariş için verilen nihai karar
        /// (Automatic / ApprovalNeeded / Cancelled)
        /// </summary>
        public OrderDecision Decision { get; set; }

        /// <summary>
        /// ONAY_GEREKLI veya IPTAL durumunda
        /// operatör için açıklama listesi
        /// </summary>
        public List<string> Reasons { get; } = new();

        /// <summary>
        /// Bu karar için SMS gönderilmeli mi?
        /// </summary>
        public bool RequiresSms =>
            Decision == OrderDecision.Automatic ||
            Decision == OrderDecision.ApprovalNeeded;
    }
}
