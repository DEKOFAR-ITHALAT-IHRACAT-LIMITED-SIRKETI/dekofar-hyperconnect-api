using Dekofar.HyperConnect.Integrations.Shopify.UseCases.Orders;
using Dekofar.HyperConnect.Integrations.Shopify.UseCases.Sms;
using Microsoft.AspNetCore.Mvc;

namespace dekofar_hyperconnect_api.Controllers.Integrations.Shopify
{
    [ApiController]
    [Route("api/integrations/shopify/orders")]
    public class ShopifyOrdersController : ControllerBase
    {
        private readonly IGetFulfilledOrdersUseCase _getFulfilledOrdersUseCase;
        private readonly ISendShippedOrdersBulkSmsUseCase _sendShippedOrdersBulkSmsUseCase;
        private readonly IPreviewShippedOrdersSmsUseCase _previewShippedOrdersSmsUseCase;

        public ShopifyOrdersController(
            IGetFulfilledOrdersUseCase getFulfilledOrdersUseCase,
            ISendShippedOrdersBulkSmsUseCase sendShippedOrdersBulkSmsUseCase,
            IPreviewShippedOrdersSmsUseCase previewShippedOrdersSmsUseCase)
        {
            _getFulfilledOrdersUseCase = getFulfilledOrdersUseCase;
            _sendShippedOrdersBulkSmsUseCase = sendShippedOrdersBulkSmsUseCase;
            _previewShippedOrdersSmsUseCase = previewShippedOrdersSmsUseCase;
        }

        /// <summary>
        /// Son 24 saat içinde kargoya verilmiş (fulfilled) siparişler
        /// </summary>
        [HttpGet("fulfilled/last-24-hours")]
        public async Task<IActionResult> Last24Hours(CancellationToken ct)
        {
            var endUtc = DateTime.UtcNow;
            var startUtc = endUtc.AddHours(-24);

            var result = await _getFulfilledOrdersUseCase
                .ExecuteAsync(startUtc, endUtc, ct);

            return Ok(result);
        }

        /// <summary>
        /// Bugün (Türkiye saati) kargoya verilmiş siparişler
        /// </summary>
        [HttpGet("fulfilled/today")]
        public async Task<IActionResult> Today(CancellationToken ct)
        {
            // Türkiye saati
            var trNow = DateTime.UtcNow.AddHours(3);

            // TR 00:00 → UTC
            var startUtc = trNow.Date.AddHours(-3);
            var endUtc = trNow;

            var result = await _getFulfilledOrdersUseCase
                .ExecuteAsync(startUtc, endUtc, ct);

            return Ok(result);
        }

        /// <summary>
        /// SMS gönderimi ÖNCESİ ön izleme (dry-run)
        /// Kimlere / kaç SMS gidecek gösterir
        /// </summary>
        [HttpGet("fulfilled/sms-preview")]
        public async Task<IActionResult> PreviewSms(CancellationToken ct)
        {
            var endUtc = DateTime.UtcNow;
            var startUtc = endUtc.AddHours(-24);

            var preview = await _previewShippedOrdersSmsUseCase
                .ExecuteAsync(startUtc, endUtc, ct);

            return Ok(new
            {
                TotalSms = preview.Count,
                Items = preview
            });
        }

        /// <summary>
        /// Fulfilled siparişler için toplu SMS gönderir
        /// (sms_sent tag’i olmayanlara)
        /// </summary>
        [HttpPost("fulfilled/send-sms")]
        public async Task<IActionResult> SendFulfilledSms(CancellationToken ct)
        {
            await _sendShippedOrdersBulkSmsUseCase.ExecuteAsync(ct);
            return Ok("📨 SMS gönderimi tamamlandı");
        }
    }
}
