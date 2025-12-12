using Dekofar.HyperConnect.Integrations.Shopify.Abstractions.Ports;
using Dekofar.HyperConnect.Integrations.Shopify.Models.Internal;
using Microsoft.Extensions.Logging;

namespace Dekofar.HyperConnect.Integrations.Shopify.UseCases.Orders
{
    public class GetFulfilledOrdersUseCase : IGetFulfilledOrdersUseCase
    {
        private readonly IShopifyOrderPort _orderPort;
        private readonly ILogger<GetFulfilledOrdersUseCase> _logger;

        public GetFulfilledOrdersUseCase(
            IShopifyOrderPort orderPort,
            ILogger<GetFulfilledOrdersUseCase> logger)
        {
            _orderPort = orderPort;
            _logger = logger;
        }

        public async Task<List<ShippedOrder>> ExecuteAsync(
            DateTime startUtc,
            DateTime endUtc,
            CancellationToken ct = default)
        {
            _logger.LogInformation(
                "📦 Fulfilled orders çekiliyor: {Start} - {End}",
                startUtc, endUtc);

            return await _orderPort.GetFulfilledOrdersAsync(
                startUtc,
                endUtc,
                ct);
        }
    }
}
