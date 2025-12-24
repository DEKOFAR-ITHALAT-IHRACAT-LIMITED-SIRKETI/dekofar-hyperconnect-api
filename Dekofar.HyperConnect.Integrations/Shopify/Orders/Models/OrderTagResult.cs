namespace Dekofar.HyperConnect.Integrations.Shopify.Orders.Models;

public class OrderTagResult
{
    public string Tag { get; set; } = null!;

    // 🔍 Kuralın sebebi (iptal, ara1 vs.)
    public string? Reason { get; set; }

    // 🧠 Öncelik (yüksek kazanır)
    public int Priority { get; set; }
}
