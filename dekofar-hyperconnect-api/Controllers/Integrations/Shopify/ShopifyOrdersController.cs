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

        public ShopifyOrdersController(
            IGetFulfilledOrdersUseCase getFulfilledOrdersUseCase,
            ISendShippedOrdersBulkSmsUseCase sendShippedOrdersBulkSmsUseCase)
        {
            _getFulfilledOrdersUseCase = getFulfilledOrdersUseCase;
            _sendShippedOrdersBulkSmsUseCase = sendShippedOrdersBulkSmsUseCase;
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
            // TR şimdi
            var trNow = DateTime.UtcNow.AddHours(3);

            // TR 00:00 → UTC
            var startUtc = trNow.Date.AddHours(-3);
            var endUtc = trNow;

            var result = await _getFulfilledOrdersUseCase
                .ExecuteAsync(startUtc, endUtc, ct);

            return Ok(result);
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
