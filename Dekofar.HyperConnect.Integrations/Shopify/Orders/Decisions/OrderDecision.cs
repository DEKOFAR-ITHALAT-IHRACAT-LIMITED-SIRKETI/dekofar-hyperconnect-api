namespace Dekofar.HyperConnect.Integrations.Shopify.Orders.Decisions;

public enum OrderDecision
{
    Automatic,      // Direkt onay
    ApprovalNeeded, // ONAY_GEREKLI
    Cancelled       // IPTAL
}
