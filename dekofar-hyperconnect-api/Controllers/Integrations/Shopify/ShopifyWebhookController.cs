using Dekofar.HyperConnect.Integrations.NetGsm.Interfaces.sms;
using Dekofar.HyperConnect.Integrations.NetGsm.Models.sms;
using Dekofar.HyperConnect.Integrations.Shopify.Utils;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;

namespace Dekofar.API.Controllers.Integrations.Shopify
{
    [ApiController]
    [Route("api/shopify/webhooks")]
    public class ShopifyWebhookController : ControllerBase
    {
        private readonly INetGsmSmsService _smsService;
        private readonly ILogger<ShopifyWebhookController> _logger;
        private readonly IConfiguration _configuration;

        public ShopifyWebhookController(
            INetGsmSmsService smsService,
            ILogger<ShopifyWebhookController> logger,
            IConfiguration configuration)
        {
            _smsService = smsService;
            _logger = logger;
            _configuration = configuration;
        }

        [HttpPost("order-created")]
        public async Task<IActionResult> OrderCreated()
        {
            try
            {
                // 🔑 Header’dan HMAC al
                var shopifyHmac = Request.Headers["X-Shopify-Hmac-Sha256"].FirstOrDefault();

                // Body’yi oku (tek sefer)
                using var reader = new StreamReader(Request.Body, Encoding.UTF8);
                var body = await reader.ReadToEndAsync();

                // HMAC doğrulama
                var secret = _configuration["Shopify:WebhookSecret"];
                if (!ShopifyHmacValidator.IsValid(secret, body, shopifyHmac))
                {
                    _logger.LogWarning("⚠️ Geçersiz Shopify HMAC. İstek reddedildi.");
                    return Unauthorized();
                }

                // ✅ JSON parse et
                var json = JsonDocument.Parse(body).RootElement;
                var customer = json.GetProperty("customer");

                var phone = customer.TryGetProperty("phone", out var p) ? p.GetString() : null;
                var firstName = customer.TryGetProperty("first_name", out var f) ? f.GetString() : "";
                var lastName = customer.TryGetProperty("last_name", out var l) ? l.GetString() : "";
                var fullName = $"{firstName} {lastName}".Trim();

                if (!string.IsNullOrWhiteSpace(phone))
                {
                    var req = new SmsSendRequest
                    {
                        MsgHeader = "DEKOFAR LTD",
                        Messages = new List<SmsMessageItem>
                        {
                            new SmsMessageItem
                            {
Msg = $"Sipariş talebiniz alınmıştır. Mesai saatleri içinde siparişinizin durumu ile ilgili size dönüş sağlanacaktır. Dekofar Müşteri Hizmetleri",
                                No = NormalizePhone(phone)
                            }
                        }
                    };

                    var result = await _smsService.SendSmsAsync(req);
                    _logger.LogInformation("✅ SMS gönderildi {Phone}, Result: {Res}", phone, result.RawResponse);
                }
                else
                {
                    _logger.LogWarning("📵 Siparişte telefon numarası bulunamadı.");
                }

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Webhook işlenirken hata oluştu.");
                return BadRequest(new { error = ex.Message });
            }
        }

        private string NormalizePhone(string raw)
        {
            var s = new string(raw.Where(char.IsDigit).ToArray());
            if (s.StartsWith("90") && s.Length == 12) s = s.Substring(2);
            if (s.StartsWith("0") && s.Length == 11) s = s.Substring(1);
            return s;
        }
    }
}
