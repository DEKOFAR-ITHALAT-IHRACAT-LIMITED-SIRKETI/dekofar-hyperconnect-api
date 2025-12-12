using Dekofar.HyperConnect.Integrations.Shopify.Models.Internal;

namespace Dekofar.HyperConnect.Integrations.Shopify.UseCases.Orders
{
    public interface IGetFulfilledOrdersUseCase
    {
        Task<List<ShippedOrder>> ExecuteAsync(
            DateTime startUtc,
            DateTime endUtc,
            CancellationToken ct = default);
    }
}
