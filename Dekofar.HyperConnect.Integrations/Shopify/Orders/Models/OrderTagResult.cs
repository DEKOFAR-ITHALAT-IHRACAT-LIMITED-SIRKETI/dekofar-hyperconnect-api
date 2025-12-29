public class OrderTagResult
{
    public string Tag { get; set; } = default!;
    public int Priority { get; set; }

    // SMS / otomasyon için
    public string? ReasonCode { get; set; }

    // İnsan için (Shopify note)
    public List<string> Notes { get; set; } = new();
}
