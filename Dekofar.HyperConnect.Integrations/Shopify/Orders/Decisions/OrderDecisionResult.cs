namespace Dekofar.HyperConnect.Integrations.Shopify.Orders.Decisions
{
    public sealed class OrderDecisionResult
    {
        public OrderDecision Decision { get; set; }

        public List<string> Reasons { get; }

        public bool IsForcedApproval { get; set; }

        // ✅ PARAMETRESİZ (İstersen manuel set edebilirsin)
        public OrderDecisionResult()
        {
            Reasons = new List<string>();
        }

        // ✅ ENGINE'İN KULLANDIĞI CONSTRUCTOR
        public OrderDecisionResult(
            OrderDecision decision,
            IEnumerable<string> reasons,
            bool isForcedApproval)
        {
            Decision = decision;
            Reasons = reasons?.ToList() ?? new List<string>();
            IsForcedApproval = isForcedApproval;
        }
    }
}
