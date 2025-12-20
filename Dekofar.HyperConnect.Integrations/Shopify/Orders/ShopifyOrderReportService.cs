using Dekofar.HyperConnect.Domain.Entities;
using Dekofar.HyperConnect.Integrations.Shopify.Clients;
using Dekofar.HyperConnect.Integrations.Shopify.Common;
using Dekofar.HyperConnect.Integrations.Shopify.Orders.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Dekofar.HyperConnect.Integrations.Shopify.Orders
{
    public class ShopifyOrderReportService
    {
        private readonly ShopifyRestClient _rest;
        private readonly ShopifyGraphQlClient _gql;

        public ShopifyOrderReportService(
            ShopifyRestClient rest,
            ShopifyGraphQlClient gql)
        {
            _rest = rest;
            _gql = gql;
        }

        /// <summary>
        /// Açık (open) siparişleri, tag filtresine göre
        /// ürün + varyant kırılımında raporlar.
        /// </summary>
        public async Task<List<OrderItemReportDto>> GetOpenOrdersByTagAsync(
            OrderItemReportFilter filter,
            CancellationToken ct = default)
        {
            // 1️⃣ Shopify search query oluştur
            var query = ShopifyQueryBuilder.Build(filter);

            // 2️⃣ Siparişleri REST ile çek
            var orders = await _rest.GetOrdersByQueryAsync(query, ct);

            var result = new List<OrderItemReportDto>();

            foreach (var order in orders)
            {
                if (order.LineItems == null || order.LineItems.Count == 0)
                    continue;

                var orderTags = string.IsNullOrWhiteSpace(order.Tags)
                    ? new List<string>()
                    : order.Tags.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                .Select(t => t.Trim())
                                .ToList();

                foreach (var item in order.LineItems)
                {
                    result.Add(new OrderItemReportDto
                    {
                        OrderId = order.Id,
                        OrderNumber = order.OrderNumber ?? order.Name,
                        OrderDate = TryParseDate(order.CreatedAt),

                        ProductTitle = item.Title,
                        VariantTitle = item.VariantTitle,

                        Quantity = item.Quantity,

                        ImageUrl = null,
                        OrderTags = orderTags
                    });

                }
            }

            return result;
        }

        private static DateTime? TryParseDate(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            return DateTime.TryParse(value, out var dt)
                ? dt
                : null;
        }
    }
}
