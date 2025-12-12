using Dekofar.HyperConnect.Integrations.Shopify.Abstractions.Ports;
using Dekofar.HyperConnect.Integrations.Shopify.UseCases.Orders;
using Dekofar.HyperConnect.Integrations.Sms.Abstractions;
using Microsoft.Extensions.Logging;

namespace Dekofar.HyperConnect.Integrations.Shopify.UseCases.Sms
{
    public class SendShippedOrdersBulkSmsUseCase
        : ISendShippedOrdersBulkSmsUseCase
    {
        private readonly IGetFulfilledOrdersUseCase _ordersUseCase;
        private readonly ISmsSender _smsSender;
        private readonly IShopifyOrderPort _shopifyPort;
        private readonly ILogger<SendShippedOrdersBulkSmsUseCase> _logger;

        private const string SmsSentTag = "sms_sent";

        public SendShippedOrdersBulkSmsUseCase(
            IGetFulfilledOrdersUseCase ordersUseCase,
            ISmsSender smsSender,
            IShopifyOrderPort shopifyPort,
            ILogger<SendShippedOrdersBulkSmsUseCase> logger)
        {
            _ordersUseCase = ordersUseCase;
            _smsSender = smsSender;
            _shopifyPort = shopifyPort;
            _logger = logger;
        }

        public async Task ExecuteAsync(CancellationToken ct = default)
        {
            var endUtc = DateTime.UtcNow;
            var startUtc = endUtc.AddHours(-24);

            var shippedOrders =
                await _ordersUseCase.ExecuteAsync(startUtc, endUtc, ct);

            foreach (var order in shippedOrders)
            {
                if (string.IsNullOrWhiteSpace(order.Phone))
                    continue;

                foreach (var tracking in order.TrackingNumbers)
                {
                    var message =
                        $"Siparişiniz kargoya verildi.\n" +
                        $"Takip No: {tracking}\n" +
                        $"Dekofar";

                    await _smsSender.SendAsync(
                        order.Phone,
                        message,
                        ct);
                }

                // ✅ SMS atıldıktan sonra Shopify'a tag bas
                await _shopifyPort.AddOrderTagAsync(
                    order.OrderId,
                    SmsSentTag,
                    ct);

                _logger.LogInformation(
                    "📨 SMS gönderildi ve tag eklendi → OrderId: {OrderId}",
                    order.OrderId);
            }
        }
    }
}
