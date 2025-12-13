using Dekofar.HyperConnect.Integrations.Shopify.Models.Internal;
using Dekofar.HyperConnect.Integrations.Shopify.UseCases.Orders;
using Microsoft.Extensions.Logging;

namespace Dekofar.HyperConnect.Integrations.Shopify.UseCases.Sms
{
    public class PreviewShippedOrdersSmsUseCase
        : IPreviewShippedOrdersSmsUseCase
    {
        private readonly IGetFulfilledOrdersUseCase _ordersUseCase;
        private readonly ILogger<PreviewShippedOrdersSmsUseCase> _logger;

        public PreviewShippedOrdersSmsUseCase(
            IGetFulfilledOrdersUseCase ordersUseCase,
            ILogger<PreviewShippedOrdersSmsUseCase> logger)
        {
            _ordersUseCase = ordersUseCase;
            _logger = logger;
        }

        public async Task<List<SmsPreviewItem>> ExecuteAsync(
            DateTime startUtc,
            DateTime endUtc,
            CancellationToken ct = default)
        {
            _logger.LogInformation(
                "📄 SMS PREVIEW → {Start} - {End}",
                startUtc, endUtc);

            var orders = await _ordersUseCase
                .ExecuteAsync(startUtc, endUtc, ct);

            var result = new List<SmsPreviewItem>();

            foreach (var order in orders)
            {
                if (string.IsNullOrWhiteSpace(order.Phone))
                    continue;

                if (order.Trackings == null || !order.Trackings.Any())
                    continue;

                foreach (var tracking in order.Trackings)
                {
                    var message = SmsMessageBuilder.Build(tracking);

                    result.Add(new SmsPreviewItem
                    {
                        OrderId = order.OrderId,
                        OrderNumber = order.OrderNumber,
                        Phone = order.Phone,
                        Carrier = tracking.Company ?? "Bilinmiyor",
                        TrackingNumber = tracking.TrackingNumber,
                        TrackingUrl = tracking.TrackingUrl,
                        Message = message
                    });
                }
            }

            _logger.LogInformation(
                "📊 SMS PREVIEW sonucu → {Count} SMS",
                result.Count);

            return result;
        }
    }
}
