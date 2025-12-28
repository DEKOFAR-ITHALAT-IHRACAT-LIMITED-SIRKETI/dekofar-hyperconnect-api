namespace Dekofar.HyperConnect.Integrations.Shopify.Orders.Models;

public class OrderTagResult
{
    public required string Tag { get; set; }

    // Birden fazla sebep
    public List<string> Reasons { get; set; } = new();

    public int Priority { get; set; }
}
