namespace Dekofar.HyperConnect.Integrations.Shopify.Orders.Models;

public class OrderTagResult
{
    public required string Tag { get; set; }

    public int Priority { get; set; }

    /// <summary>
    /// Shopify sipariş NOTU
    /// (müşteri notunu ezmez, sistem notu olarak eklenir)
    /// </summary>
    public string? Note { get; set; }
}

