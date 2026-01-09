using Dekofar.HyperConnect.Application.Integrations.Shopify.Services;
using Dekofar.HyperConnect.Integrations.Shopify.Interfaces;
using Dekofar.HyperConnect.Integrations.Shopify.Models.Shopify;
using Dekofar.HyperConnect.Integrations.Shopify.Models.Shopify.Dto;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Text.Json;

namespace Dekofar.HyperConnect.Integrations.Shopify.Services
{
    public class ShopifyService : IShopifyService
    {
        private readonly HttpClient _httpClient;
        private readonly ShopifyStoreService _storeService;
        private readonly ILogger<ShopifyService> _logger;

        public ShopifyService(
            HttpClient httpClient,
            ShopifyStoreService storeService,
            ILogger<ShopifyService> logger)
        {
            _httpClient = httpClient;
            _storeService = storeService;
            _logger = logger;
        }

        // =====================================================
        // 🔐 Client hazırlığı (HER REQUEST ÖNCESİ)
        // =====================================================
        private async Task PrepareClientAsync(string shopDomain, CancellationToken ct)
        {
            var store = await _storeService.GetByShopDomainAsync(shopDomain, ct);

            if (store == null)
                throw new InvalidOperationException($"Shop not installed: {shopDomain}");

            _httpClient.BaseAddress =
                new Uri($"https://{store.ShopDomain}/admin/api/2024-01/");

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add(
                "X-Shopify-Access-Token",
                store.AccessToken
            );
        }

        // =====================================================
        // 📦 SON 10 SİPARİŞ
        // =====================================================
        public async Task<IReadOnlyList<object>> GetLatestOrdersAsync(
            string shopDomain,
            int limit,
            CancellationToken ct)
        {
            await PrepareClientAsync(shopDomain, ct);

            using var response =
                await _httpClient.GetAsync(
                    $"orders.json?limit={limit}&status=any",
                    ct
                );

            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync(ct);

            using var doc = JsonDocument.Parse(json);

            var orders =
                doc.RootElement
                   .GetProperty("orders")
                   .Deserialize<List<object>>();

            return orders ?? new List<object>();
        }

        // =====================================================
        // 🧪 CONNECTION TEST
        // =====================================================
        public async Task<string> TestConnectionAsync(CancellationToken ct = default)
        {
            using var response = await _httpClient.GetAsync("shop.json", ct);
            response.EnsureSuccessStatusCode();
            return "OK";
        }

        // =====================================================
        // ⛔ AŞAĞIDAKİLER ŞU AN BİLİNÇLİ OLARAK BOŞ
        // =====================================================

        public Task<PagedResult<Order>> GetOrdersPagedAsync(string? pageInfo = null, int limit = 10, CancellationToken ct = default)
            => throw new NotImplementedException();

        public Task<Order?> GetOrderByIdAsync(long orderId, CancellationToken ct = default)
            => throw new NotImplementedException();

        public Task<ShopifyOrderDetailDto?> GetOrderDetailWithImagesAsync(long orderId, CancellationToken ct = default)
            => throw new NotImplementedException();

        public Task<List<ShopifyProduct>> GetAllProductsAsync(CancellationToken ct = default)
            => throw new NotImplementedException();

        public Task<ShopifyProduct?> GetProductByIdAsync(long productId, CancellationToken ct = default)
            => throw new NotImplementedException();

        public Task<List<ShopifyProduct>> SearchProductsAsync(string query, CancellationToken ct = default)
            => throw new NotImplementedException();

        public Task<ShopifyVariant?> GetVariantByIdAsync(long variantId, CancellationToken ct = default)
            => throw new NotImplementedException();

        public Task<List<ShopifyVariant>> GetVariantsByProductIdAsync(long productId, CancellationToken ct = default)
            => throw new NotImplementedException();

        public Task<List<ShopifyProduct>> GetLowStockProductsAsync(int threshold, CancellationToken ct = default)
            => throw new NotImplementedException();

        public Task<bool> AddOrUpdateProductTagsAsync(long productId, string tags, CancellationToken ct = default)
            => throw new NotImplementedException();

        public Task<List<Order>> SearchOrdersAsync(OrderSearchFilter filter, CancellationToken ct = default)
            => throw new NotImplementedException();

        public Task<List<Order>> GetOrdersBySearchQueryAsync(string query, CancellationToken ct = default)
            => throw new NotImplementedException();

        public Task<bool> UpdateOrderTagsAsync(long orderId, string tags, CancellationToken ct = default)
            => throw new NotImplementedException();

        public Task<bool> UpdateOrderNoteAsync(long orderId, string note, CancellationToken ct = default)
            => throw new NotImplementedException();

        public Task<Customer?> GetCustomerByIdAsync(long customerId, CancellationToken ct = default)
            => throw new NotImplementedException();

        public Task<Order?> CreateOrderAsync(Order order, CancellationToken ct = default)
            => throw new NotImplementedException();

        public Task<string> CreateFulfillmentAsync(long orderId, FulfillmentCreateRequest request, CancellationToken ct = default)
            => throw new NotImplementedException();

        public Task<List<Order>> SearchOrdersWithDetailsAsync(string query, CancellationToken ct = default)
            => throw new NotImplementedException();

        public Task<PagedResult<Order>> GetOpenOrdersWithCursorAsync(string? pageInfo, int limit, CancellationToken ct)
            => throw new NotImplementedException();

        public Task<List<ShopifyOrderLiteDto>> SearchOrdersLiteAsync(string query, CancellationToken ct = default)
            => throw new NotImplementedException();

        public Task<long?> GetOrderIdByTrackingNumberAsync(string trackingNumber, CancellationToken cancellationToken = default)
            => throw new NotImplementedException();

        public Task<bool> MarkOrderAsPaidAsync(long orderId, CancellationToken ct = default)
            => throw new NotImplementedException();

        public Task<List<ShopifyOrderItemSummaryDto>> GetOrderItemsSummaryAsync(
            DateTime? start = null,
            DateTime? end = null,
            string? financialCsv = null,
            string? fulfillmentCsv = null,
            string? statusCsv = null,
            CancellationToken ct = default)
            => throw new NotImplementedException();
    }
}
