namespace Dekofar.HyperConnect.Integrations.Shopify.Orders.Decisions
{
    public class AddressValidationResult
    {
        public bool IsValid { get; set; }

        public List<string> Reasons { get; } = new();
    }
}
