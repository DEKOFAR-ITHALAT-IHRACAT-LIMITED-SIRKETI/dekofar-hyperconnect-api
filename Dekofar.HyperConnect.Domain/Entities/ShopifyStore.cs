public class ShopifyStore
{
    public Guid Id { get; set; }

    public string ShopDomain { get; set; } = null!;
    public string AccessToken { get; set; } = null!;
    public string Scopes { get; set; } = null!;

    public DateTime InstalledAtUtc { get; set; }
    public bool IsActive { get; set; }
}
