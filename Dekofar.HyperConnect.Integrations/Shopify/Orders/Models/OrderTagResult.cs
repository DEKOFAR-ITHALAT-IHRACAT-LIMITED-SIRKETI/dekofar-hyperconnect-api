namespace Dekofar.HyperConnect.Integrations.Shopify.Orders.Models;

/// <summary>
/// Bir sipariş için hesaplanan TEK etiket sonucu
/// </summary>
public class OrderTagResult
{
    /// <summary>
    /// Atanacak etiket (tek)
    /// Örn: dhl, ptt, ara1, iptal
    /// </summary>
    public required string Tag { get; set; }

    /// <summary>
    /// Log / debug amacıyla sebep
    /// </summary>
    public string? Reason { get; set; }
    public int Priority { get; set; }


    /// <summary>
    /// Shopify sipariş NOTU
    /// (müşteri notunu ezmez, sistem notu olarak eklenir)
    /// </summary>
    public string? Note { get; set; }
}
