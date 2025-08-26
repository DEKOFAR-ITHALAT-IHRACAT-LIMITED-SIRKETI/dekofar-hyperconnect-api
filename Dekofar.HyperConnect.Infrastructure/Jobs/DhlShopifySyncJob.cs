using Dekofar.HyperConnect.Application.Common.Interfaces;
using Dekofar.HyperConnect.Infrastructure.Services;
using Dekofar.HyperConnect.Integrations.Kargo.Dhl.Interfaces;
using Dekofar.HyperConnect.Integrations.Shopify.Interfaces;
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

        public DhlShopifySyncJob(
            IDhlKargoDeliveredShipmentService dhlService,
            IShopifyService shopifyService,
            IJobStatsService statsService)
        {
            _dhlService = dhlService;
            _shopifyService = shopifyService;
            _statsService = statsService;
        }

        /// <summary>
        /// Günlük DHL teslimatlarını kontrol eder ve Shopify siparişlerini günceller.
        /// </summary>
        public async Task RunAsync(CancellationToken cancellationToken = default)
        {
            var todayDeliveries = await _dhlService.GetDeliveredShipmentsByDateAsync(DateTime.Today);

            foreach (var delivery in todayDeliveries)
            {
                if (delivery.shipment?.referenceId == null)
                    continue;

                if (!long.TryParse(delivery.shipment.referenceId, out var shopifyOrderId))
                    continue;

                var code = delivery.shipment?.shipmentStatusCode;

                if (code == 5) // DHL: Ödendi
                {
                    var ok = await _shopifyService.MarkOrderAsPaidAsync(shopifyOrderId, cancellationToken);
                    if (ok)
                        await _statsService.IncrementPaidMarkedAsync(cancellationToken);
                }
                else if (code == 7) // DHL: İptal
                {
                    var ok = await _shopifyService.UpdateOrderTagsAsync(shopifyOrderId, "İptal", cancellationToken);
                    if (ok)
                        await _statsService.IncrementCancelTaggedAsync(cancellationToken);
                }
            }
        }
    }
}
