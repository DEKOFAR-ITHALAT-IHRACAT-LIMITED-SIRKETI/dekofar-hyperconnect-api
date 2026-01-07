namespace Dekofar.HyperConnect.Integrations.Shopify.Common;

public class ShopifyOptions
{
    public string ClientId { get; set; } = null!;
    public string ClientSecret { get; set; } = null!;
    public string Scopes { get; set; } = null!;
    public string AppUrl { get; set; } = null!;
}
