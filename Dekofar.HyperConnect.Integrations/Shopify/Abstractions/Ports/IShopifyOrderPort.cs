// Abstractions/Ports/IShopifyOrderPort.cs
using Dekofar.HyperConnect.Integrations.Shopify.Models.Internal;

namespace Dekofar.HyperConnect.Integrations.Shopify.Abstractions.Ports
{
    public interface IShopifyOrderPort
    {
        Task<List<ShippedOrder>> GetFulfilledOrdersAsync(
            DateTime startUtc,
            DateTime endUtc,
            CancellationToken ct = default);

        Task AddOrderTagAsync(long orderId, string tag, CancellationToken ct);



    }


}
