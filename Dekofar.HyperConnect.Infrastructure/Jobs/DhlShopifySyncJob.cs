using Dekofar.HyperConnect.Application.Common.Interfaces;
using Dekofar.HyperConnect.Infrastructure.Services;
using Dekofar.HyperConnect.Integrations.Kargo.Dhl.Interfaces;
using Dekofar.HyperConnect.Integrations.Shopify.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Dekofar.HyperConnect.Infrastructure.Jobs
{
    /// <summary>
    /// DHL teslimat statülerini Shopify siparişleri ile senkronize eder.
    /// </summary>
    public class DhlShopifySyncJob : IRecurringJob
    {
        private readonly IDhlKargoDeliveredShipmentService _dhlService;
        private readonly IShopifyService _shopifyService;
        private readonly IJobStatsService _statsService;
        private readonly ILogger<DhlShopifySyncJob> _logger;

        public DhlShopifySyncJob(
            IDhlKargoDeliveredShipmentService dhlService,
            IShopifyService shopifyService,
            IJobStatsService statsService,
            ILogger<DhlShopifySyncJob> logger)
        {
            _dhlService = dhlService;
            _shopifyService = shopifyService;
            _statsService = statsService;
            _logger = logger;
        }

        /// <summary>
        /// Günlük DHL teslimatlarını kontrol eder ve Shopify siparişlerini günceller.
        /// </summary>
        public async Task RunAsync(CancellationToken cancellationToken = default)
        {
            var todayDeliveries = await _dhlService.GetDeliveredShipmentsByDateAsync(DateTime.Today);

            foreach (var delivery in todayDeliveries)
            {
                var trackingNumber = delivery.shipment?.shipmentId; // DHL gönderi no
                var code = delivery.shipment?.shipmentStatusCode;

                if (string.IsNullOrEmpty(trackingNumber))
                    continue;

                // Shopify siparişini tracking no ile bul
                var shopifyOrderId = await _shopifyService.GetOrderIdByTrackingNumberAsync(trackingNumber, cancellationToken);
                if (shopifyOrderId == null)
                {
                    _logger.LogWarning("⚠️ Eşleşen Shopify siparişi bulunamadı. TrackingNo: {TrackingNo}", trackingNumber);
                    continue;
                }

                if (code == 5) // DHL: Ödendi
                {
                    var ok = await _shopifyService.MarkOrderAsPaidAsync(shopifyOrderId.Value, cancellationToken);
                    if (ok)
                    {
                        await _statsService.IncrementPaidMarkedAsync(cancellationToken);
                        _logger.LogInformation("✅ Sipariş {OrderId} 'Paid' işaretlendi. TrackingNo: {TrackingNo}", shopifyOrderId, trackingNumber);
                    }
                }
                else if (code == 7) // DHL: İptal
                {
                    var ok = await _shopifyService.UpdateOrderTagsAsync(shopifyOrderId.Value, "İptal", cancellationToken);
                    if (ok)
                    {
                        await _statsService.IncrementCancelTaggedAsync(cancellationToken);
                        _logger.LogInformation("❌ Sipariş {OrderId} 'İptal' etiketlendi. TrackingNo: {TrackingNo}", shopifyOrderId, trackingNumber);
                    }
                }
            }
        }

        public async Task<List<(string TrackingNumber, long? ShopifyOrderId, bool Success, string? Error)>>
    RunForDateAsync(DateTime date, CancellationToken ct = default)
        {
            var results = new List<(string, long?, bool, string?)>();
            var deliveries = await _dhlService.GetDeliveredShipmentsByDateAsync(date);

            foreach (var delivery in deliveries)
            {
                var trackingNumber = delivery.shipment?.shipmentId;
                var code = delivery.shipment?.shipmentStatusCode;

                if (string.IsNullOrEmpty(trackingNumber))
                {
                    results.Add((trackingNumber ?? "-", null, false, "Tracking numarası boş"));
                    continue;
                }

                try
                {
                    var shopifyOrderId = await _shopifyService.GetOrderIdByTrackingNumberAsync(trackingNumber, ct);

                    if (shopifyOrderId == null)
                    {
                        results.Add((trackingNumber, null, false, "Shopify siparişi bulunamadı"));
                        continue;
                    }

                    if (code == 5) // DHL: Ödendi
                    {
                        var ok = await _shopifyService.MarkOrderAsPaidAsync(shopifyOrderId.Value, ct);
                        if (ok)
                        {
                            await _statsService.IncrementPaidMarkedAsync(ct);
                            results.Add((trackingNumber, shopifyOrderId, true, null));
                        }
                        else
                        {
                            results.Add((trackingNumber, shopifyOrderId, false, "Shopify ödeme işaretleme başarısız"));
                        }
                    }
                    else if (code == 7) // DHL: İptal
                    {
                        var ok = await _shopifyService.UpdateOrderTagsAsync(shopifyOrderId.Value, "İptal", ct);
                        if (ok)
                        {
                            await _statsService.IncrementCancelTaggedAsync(ct);
                            results.Add((trackingNumber, shopifyOrderId, true, null));
                        }
                        else
                        {
                            results.Add((trackingNumber, shopifyOrderId, false, "İptal etiketi eklenemedi"));
                        }
                    }
                    else
                    {
                        results.Add((trackingNumber, shopifyOrderId, false, $"Beklenmeyen status code: {code}"));
                    }
                }
                catch (Exception ex)
                {
                    results.Add((trackingNumber, null, false, ex.Message));
                }
            }

            return results;
        }

    }
}
