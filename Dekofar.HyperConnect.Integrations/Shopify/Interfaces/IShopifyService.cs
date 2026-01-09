using Dekofar.HyperConnect.Domain.Entities;
using Dekofar.HyperConnect.Integrations.Shopify.Models.Shopify;
using Dekofar.HyperConnect.Integrations.Shopify.Models.Shopify.Dto;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Dekofar.HyperConnect.Integrations.Shopify.Interfaces
{
    public interface IShopifyService
    {
        Task<string> TestConnectionAsync(CancellationToken cancellationToken = default);

        // ✅ SON SİPARİŞLER (DOĞRU İMZA)
        Task<IReadOnlyList<ShopifyOrderDto>> GetLatestOrdersAsync(
            string shopDomain,
            int limit,
            CancellationToken ct);

        // -------------------------------------------------
        // ⛔ Aşağıdakiler şimdilik boş (bilinçli)
        // -------------------------------------------------

        Task<PagedResult<Order>> GetOrdersPagedAsync(string? pageInfo = null, int limit = 10, CancellationToken ct = default);
        Task<Order?> GetOrderByIdAsync(long orderId, CancellationToken ct = default);
        Task<ShopifyOrderDetailDto?> GetOrderDetailWithImagesAsync(long orderId, CancellationToken ct = default);
        Task<List<ShopifyProduct>> GetAllProductsAsync(CancellationToken ct = default);
        Task<ShopifyProduct?> GetProductByIdAsync(long productId, CancellationToken ct = default);
        Task<List<ShopifyProduct>> SearchProductsAsync(string query, CancellationToken ct = default);
        Task<ShopifyVariant?> GetVariantByIdAsync(long variantId, CancellationToken ct = default);
        Task<List<ShopifyVariant>> GetVariantsByProductIdAsync(long productId, CancellationToken ct = default);
        Task<List<ShopifyProduct>> GetLowStockProductsAsync(int threshold, CancellationToken ct = default);
        Task<bool> AddOrUpdateProductTagsAsync(long productId, string tags, CancellationToken ct = default);
        Task<List<Order>> SearchOrdersAsync(OrderSearchFilter filter, CancellationToken ct = default);
        Task<List<Order>> GetOrdersBySearchQueryAsync(string query, CancellationToken ct = default);
        Task<bool> UpdateOrderTagsAsync(long orderId, string tags, CancellationToken ct = default);
        Task<bool> UpdateOrderNoteAsync(long orderId, string note, CancellationToken ct = default);
        Task<Customer?> GetCustomerByIdAsync(long customerId, CancellationToken ct = default);
        Task<Order?> CreateOrderAsync(Order order, CancellationToken ct = default);
        Task<string> CreateFulfillmentAsync(long orderId, FulfillmentCreateRequest request, CancellationToken ct = default);
        Task<List<Order>> SearchOrdersWithDetailsAsync(string query, CancellationToken ct = default);
        Task<PagedResult<Order>> GetOpenOrdersWithCursorAsync(string? pageInfo, int limit, CancellationToken ct);
        Task<List<ShopifyOrderLiteDto>> SearchOrdersLiteAsync(string query, CancellationToken ct = default);
        Task<long?> GetOrderIdByTrackingNumberAsync(string trackingNumber, CancellationToken cancellationToken = default);
        Task<bool> MarkOrderAsPaidAsync(long orderId, CancellationToken ct = default);

        Task<List<ShopifyOrderItemSummaryDto>> GetOrderItemsSummaryAsync(
            DateTime? start = null,
            DateTime? end = null,
            string? financialCsv = null,
            string? fulfillmentCsv = null,
            string? statusCsv = null,
            CancellationToken ct = default);
    }
}
