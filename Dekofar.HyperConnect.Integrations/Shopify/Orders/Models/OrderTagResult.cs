namespace Dekofar.HyperConnect.Integrations.Shopify.Orders.Models;

/// <summary>
/// Bir sipariş için hesaplanan TEK etiket sonucu
/// </summary>
public class OrderTagResult
{
    /// <summary>
    /// Atanacak etiket (ara1, dhl, ptt, vb.)
    /// </summary>
    public required string Tag { get; set; }

    /// <summary>
    /// Kural önceliği (yüksek olan kazanır)
    /// </summary>
    public int Priority { get; set; } = 0;

    /// <summary>
    /// ARA1 vb. durumlar için BİRDEN FAZLA sebep
    /// </summary>
    public List<string> Reasons { get; } = new();

    /// <summary>
    /// Shopify sipariş notuna yazılacak sistem notları
    /// </summary>
    public List<string> Notes { get; } = new();
}
