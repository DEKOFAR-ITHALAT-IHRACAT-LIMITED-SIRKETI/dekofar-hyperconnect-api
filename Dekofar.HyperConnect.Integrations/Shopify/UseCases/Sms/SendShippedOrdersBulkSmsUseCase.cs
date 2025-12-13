using Dekofar.HyperConnect.Integrations.Shopify.Abstractions.Ports;
using Dekofar.HyperConnect.Integrations.Shopify.Models.Internal;
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

                if (order.Trackings == null || !order.Trackings.Any())
                    continue;

                foreach (var tracking in order.Trackings)
                {
                    var message = BuildSmsMessage(tracking);

                    var result = await _smsSender.SendAsync(
                        order.Phone,
                        message,
                        ct);

                    // ✅ SADECE BAŞARILI SMS’TE TAG BAS
                    if (result.Success)
                    {
                        await _shopifyPort.AddOrderTagAsync(
                            order.OrderId,
                            SmsSentTag,
                            ct);

                        _logger.LogInformation(
                            "📨 SMS başarılı → sms_sent eklendi → OrderId: {OrderId}",
                            order.OrderId);

                        break; // 🔒 Aynı sipariş için başka tracking'e SMS atma
                    }

                    _logger.LogWarning(
                        "❌ SMS başarısız → OrderId: {OrderId}, Code: {Code}, Desc: {Desc}",
                        order.OrderId,
                        result.Code,
                        result.Description);
                }
            }
        }

        /// <summary>
        /// Kargo firmasına göre KISA SMS metni üretir (DHL / PTT / Fallback)
        /// </summary>
        private static string BuildSmsMessage(ShippedTracking tracking)
        {
            var company = tracking.Company?.ToLowerInvariant() ?? "";

            // ✅ DHL
            // ✅ DHL
            if (company.Contains("dhl"))
            {
                return
            $@"Siparisiniz kargoya verilmistir.
DHL | Takip No: {tracking.TrackingNumber}
Takip: https://www.dhlecommerce.com.tr/gonderitakip

Siparisiniz icin tesekkur ederiz.
Dekofar.com";
            }


            // ✅ PTT
            if (company.Contains("ptt"))
            {
                return
$@"Siparisiniz kargoya verilmistir.
PTT | Takip No: {tracking.TrackingNumber}
Takip: {tracking.TrackingUrl ?? "https://gonderitakip.ptt.gov.tr/"}

Siparisiniz icin tesekkur ederiz.
Dekofar.com";
            }

            // 🟡 Fallback – bilinmeyen kargo
            return
$@"Siparisiniz kargoya verilmistir.
Takip No: {tracking.TrackingNumber}
Takip: {tracking.TrackingUrl ?? "-"}

Siparisiniz icin tesekkur ederiz.
Dekofar.com";
        }
    }
}
