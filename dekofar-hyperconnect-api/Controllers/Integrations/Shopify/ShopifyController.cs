using Dekofar.HyperConnect.Integrations.Shopify.Interfaces;
using Dekofar.HyperConnect.Integrations.Shopify.Models;
using Dekofar.HyperConnect.Integrations.Shopify.Models.Shopify;
using Dekofar.HyperConnect.Integrations.Shopify.Models.Shopify.Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using System.Text;

namespace Dekofar.API.Controllers.Integrations
{
    [ApiController]
    [Route("api/[controller]")]
    // Shopify entegrasyonu için kullanılan controller
    public class ShopifyController : ControllerBase
    {
        // Shopify servisleri ile iletişim kuran servis
        private readonly IShopifyService _shopifyService;
        private readonly ILogger<ShopifyController> _logger;

        public ShopifyController(IShopifyService shopifyService, ILogger<ShopifyController> logger)
        {
            _shopifyService = shopifyService;
            _logger = logger;
        }

        // Shopify API bağlantısını test eder
        [HttpGet("test-connection")]
        public async Task<IActionResult> TestConnection(CancellationToken ct)
        {
            var result = await _shopifyService.TestConnectionAsync(ct);
            return Ok(result);
        }

        // Sayfalama ile siparişleri listeler
        [HttpGet("orders-paged")]
        public async Task<IActionResult> GetOrdersPaged([FromQuery] string? pageInfo, [FromQuery] int limit = 10, CancellationToken ct = default)
        {
            var result = await _shopifyService.GetOrdersPagedAsync(pageInfo, limit, ct);
            return Ok(result);
        }

        // Belirli bir siparişi ID ile getirir
        [HttpGet("orders/{orderId:long}")]
        public async Task<IActionResult> GetOrderById(long orderId, CancellationToken ct)
        {
            var order = await _shopifyService.GetOrderByIdAsync(orderId, ct);
            if (order == null)
                return NotFound();
            return Ok(order);
        }

        // Sipariş detayını ürün görselleri ile birlikte getirir
        [HttpGet("order-detail/{orderId:long}")]
        public async Task<IActionResult> GetOrderDetailWithImages(long orderId, CancellationToken ct)
        {
            var detail = await _shopifyService.GetOrderDetailWithImagesAsync(orderId, ct);
            if (detail == null)
                return NotFound();
            return Ok(detail);
        }

        // Tüm ürünleri listeler
        [HttpGet("products")]
        public async Task<IActionResult> GetAllProducts(CancellationToken ct)
        {
            var products = await _shopifyService.GetAllProductsAsync(ct);
            return Ok(products);
        }

        // Belirli bir ürünü ID ile getirir
        [HttpGet("products/{productId:long}")]
        public async Task<IActionResult> GetProductById(long productId, CancellationToken ct)
        {
            var product = await _shopifyService.GetProductByIdAsync(productId, ct);
            if (product == null)
                return NotFound();
            return Ok(product);
        }

        // Ürünleri isim veya terime göre arar
        [HttpGet("products/search")]
        public async Task<IActionResult> SearchProducts([FromQuery] string query, CancellationToken ct)
        {
            var result = await _shopifyService.SearchProductsAsync(query, ct);
            return Ok(result);
        }

        // Variant bilgilerini ID ile getirir
        [HttpGet("variants/{variantId:long}")]
        public async Task<IActionResult> GetVariantById(long variantId, CancellationToken ct)
        {
            var variant = await _shopifyService.GetVariantByIdAsync(variantId, ct);
            if (variant == null)
                return NotFound();
            return Ok(variant);
        }

        // Bir ürüne ait tüm variantları listeler
        [HttpGet("products/{productId:long}/variants")]
        public async Task<IActionResult> GetVariantsByProductId(long productId, CancellationToken ct)
        {
            var variants = await _shopifyService.GetVariantsByProductIdAsync(productId, ct);
            return Ok(variants);
        }

        // Stok seviyesi düşük ürünleri listeler
        [HttpGet("products/low-stock")]
        public async Task<IActionResult> GetLowStockProducts([FromQuery] int threshold = 5, CancellationToken ct = default)
        {
            var products = await _shopifyService.GetLowStockProductsAsync(threshold, ct);
            return Ok(products);
        }

        // Ürünün etiketlerini günceller
        [HttpPut("products/{productId:long}/tags")]
        public async Task<IActionResult> AddOrUpdateProductTags(long productId, [FromQuery] string tags, CancellationToken ct)
        {
            var success = await _shopifyService.AddOrUpdateProductTagsAsync(productId, tags, ct);
            if (success)
                return Ok(new { message = "Etiketler başarıyla güncellendi." });
            return BadRequest(new { message = "Etiket güncelleme başarısız oldu." });
        }

        // Sipariş etiketlerini günceller
        [HttpPost("order/update-tags")]
        public async Task<IActionResult> UpdateOrderTags([FromBody] UpdateOrderTagsRequest request, CancellationToken ct)
        {
            var ok = await _shopifyService.UpdateOrderTagsAsync(request.OrderId, request.Tags, ct);
            return ok ? Ok() : BadRequest();
        }

        // Sipariş notunu günceller
        [HttpPost("order/update-note")]
        public async Task<IActionResult> UpdateOrderNote([FromBody] UpdateOrderNoteRequest request, CancellationToken ct)
        {
            var ok = await _shopifyService.UpdateOrderNoteAsync(request.OrderId, request.Note, ct);
            return ok ? Ok() : BadRequest();
        }

        // Müşteri bilgilerini ID ile getirir
        [HttpGet("customer/{customerId:long}")]
        public async Task<IActionResult> GetCustomer(long customerId, CancellationToken ct)
        {
            var customer = await _shopifyService.GetCustomerByIdAsync(customerId, ct);
            if (customer == null) return NotFound();
            return Ok(customer);
        }

        // Yeni bir sipariş oluşturur
        [HttpPost("order")]
        public async Task<IActionResult> CreateOrder([FromBody] Order order, CancellationToken ct)
        {
            var result = await _shopifyService.CreateOrderAsync(order, ct);
            return result != null ? Ok(result) : BadRequest();
        }

        // Sipariş için teslimat (fulfillment) oluşturur
        [HttpPost("fulfillment")]
        public async Task<IActionResult> CreateFulfillment([FromBody] FulfillmentCreateRequest request, CancellationToken ct)
        {
            var resp = await _shopifyService.CreateFulfillmentAsync(request.OrderId, request, ct);
            return Ok(resp);
        }
        [HttpGet("orders/search-lite")]
        public async Task<IActionResult> SearchOrdersLite([FromQuery] string query, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
                return BadRequest("Arama sorgusu en az 2 karakter olmalıdır.");

            try
            {
                var results = await _shopifyService.SearchOrdersLiteAsync(query, ct);
                return Ok(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "🔍 Shopify lite arama hatası: {Query}", query);
                return StatusCode(500, "Lite arama sırasında hata oluştu.");
            }
        }

        /// <summary>
        /// Shopify siparişlerinde arama yapar (isim, telefon, e-posta, etiket vb.)
        /// Arka planda hem GraphQL hem REST API birleşimi ile gelişmiş detaylar getirir.
        /// </summary>
        /// <param name="query">Arama sorgusu (örnek: müşteri adı, telefon, e-posta, etiket)</param>
        /// <param name="ct">İptal tokeni</param>
        /// <returns>Detaylı sipariş listesi</returns>
        [HttpGet("orders/search")]
        public async Task<IActionResult> SearchOrders([FromQuery] string query, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
                return BadRequest("Arama sorgusu en az 2 karakter olmalıdır.");

            try
            {
                var orders = await _shopifyService.SearchOrdersWithDetailsAsync(query, ct);
                return Ok(orders);
            }
            catch (Exception ex)
            {
                // Hata loglama
                _logger.LogError(ex, "🔍 Shopify sipariş arama hatası: {Query}", query);
                return StatusCode(500, "Shopify sipariş araması sırasında bir hata oluştu.");
            }
        }


        // Açık siparişleri cursor bazlı olarak getirir
        [HttpGet("orders-open-cursor")]
        public async Task<IActionResult> GetOpenOrdersWithCursor([FromQuery] string? pageInfo, [FromQuery] int limit = 20, CancellationToken ct = default)
        {
            var result = await _shopifyService.GetOpenOrdersWithCursorAsync(pageInfo, limit, ct);
            return Ok(result);
        }

        // Shopify sipariş cache'ini temizler
        [HttpPost("orders/clear-cache")]
        public IActionResult ClearOrderCache([FromServices] IMemoryCache memoryCache)
        {
            memoryCache.Remove("shopify_orders_cache");
            return Ok(new { message = "🧹 Cache temizlendi." });
        }

        /// <summary>
        /// Son X gün için (varsayılan 30) gönderilmemiş/kısmen gönderilmiş ürün bazlı sipariş özeti.
        /// İptal edilmiş ve financial_status=voided siparişler her zaman hariç tutulur.
        /// </summary>
        [HttpGet("order-items/summary")]
        [ProducesResponseType(typeof(List<ShopifyOrderItemSummaryDto>), StatusCodes.Status200OK)]
        [ResponseCache(Duration = 30, Location = ResponseCacheLocation.Any,
            VaryByQueryKeys = new[] { "days", "status", "financial", "fulfillment" })]
        public async Task<IActionResult> GetOrderItemsSummary(
            [FromQuery] int days = 7,
            [FromQuery] string? status = "open",
            [FromQuery] string? financial = "pending,authorized,paid,partially_paid,partially_refunded",
            [FromQuery] string? fulfillment = "unfulfilled,partial",
            CancellationToken ct = default)
        {
            // Guardrails
            if (days <= 0) days = 7;
            if (days > 365) days = 365;

            var end = DateTime.UtcNow;
            var start = end.AddDays(-Math.Abs(days));

            try
            {
                var items = await _shopifyService.GetOrderItemsSummaryAsync(
                    start: start,
                    end: end,
                    financialCsv: financial,
                    fulfillmentCsv: fulfillment,
                    statusCsv: status,
                    ct: ct);

                return Ok(items);
            }
            catch (OperationCanceledException)
            {
                // İstemci bağlantıyı kapattı vb.
                return StatusCode(StatusCodes.Status499ClientClosedRequest);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "GET /order-items/summary failed. days={Days}, status={Status}, financial={Financial}, fulfillment={Fulfillment}",
                    days, status, financial, fulfillment);

                return Problem("Shopify sipariş özeti alınamadı.");
            }
        }

        /// <summary>
        /// İsteğe bağlı: Doğrudan tarih aralığı vererek özet almak için alternatif uç.
        /// </summary>
        [HttpGet("order-items/summary-by-range")]
        [ProducesResponseType(typeof(List<ShopifyOrderItemSummaryDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetOrderItemsSummaryByRange(
            [FromQuery] DateTime? start = null,
            [FromQuery] DateTime? end = null,
            [FromQuery] string? status = "open",
            [FromQuery] string? financial = "pending,authorized,paid,partially_paid,partially_refunded",
            [FromQuery] string? fulfillment = "unfulfilled,partial",
            CancellationToken ct = default)
        {
            var endUtc = (end ?? DateTime.UtcNow);
            var startUtc = (start ?? endUtc.AddDays(-30));

            try
            {
                var items = await _shopifyService.GetOrderItemsSummaryAsync(
                    start: startUtc,
                    end: endUtc,
                    financialCsv: financial,
                    fulfillmentCsv: fulfillment,
                    statusCsv: status,
                    ct: ct);

                return Ok(items);
            }
            catch (OperationCanceledException)
            {
                return StatusCode(StatusCodes.Status499ClientClosedRequest);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GET /order-items/summary-by-range failed. start={Start}, end={End}", start, end);
                return Problem("Shopify sipariş özeti alınamadı.");
            }
        }
    }


}