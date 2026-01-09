using System;

namespace Dekofar.HyperConnect.Domain.Entities;

public class ShopifyStore
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string ShopDomain { get; set; } = null!;
    public string AccessToken { get; set; } = null!;
    public string Scopes { get; set; } = null!;

    public DateTime InstalledAtUtc { get; set; } = DateTime.UtcNow;

    public bool IsActive { get; set; } = true;
}
