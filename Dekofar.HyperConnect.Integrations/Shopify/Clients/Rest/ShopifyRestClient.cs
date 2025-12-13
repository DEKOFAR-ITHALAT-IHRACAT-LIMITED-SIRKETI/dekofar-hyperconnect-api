using Dekofar.HyperConnect.Integrations.Shopify.Abstractions.Ports;
using Dekofar.HyperConnect.Integrations.Shopify.Models.Internal;
using Dekofar.HyperConnect.Integrations.Shopify.Models.Raw;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;

namespace Dekofar.HyperConnect.Integrations.Shopify.Clients.Rest;

public class ShopifyRestClient : IShopifyOrderPort
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ShopifyRestClient> _logger;

    public ShopifyRestClient(
        HttpClient httpClient,
        IConfiguration config,
        ILogger<ShopifyRestClient> logger)
    {
        _logger = logger;
        _httpClient = httpClient;

        _httpClient.BaseAddress = new Uri(config["Shopify:BaseUrl"]!);
        _httpClient.DefaultRequestHeaders.Clear();

        _httpClient.DefaultRequestHeaders.Add(
            "X-Shopify-Access-Token",
            config["Shopify:AccessToken"]!);

        _httpClient.DefaultRequestHeaders.Accept
            .Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    /// <summary>
    /// Belirtilen zaman aralığında fulfillment (kargoya veriliş) tarihi olan siparişleri getirir.
    /// ✔ created_at DEĞİL
    /// ✔ fulfillment.created_at baz alınır
    /// ✔ sms_sent tag’i olanlar Shopify tarafında otomatik elenir
    /// </summary>
    public async Task<List<ShippedOrder>> GetFulfilledOrdersAsync(
        DateTime startUtc,
        DateTime endUtc,
        CancellationToken ct = default)
    {
        // Shopify order search: sms_sent tag hariç
        var query = Uri.EscapeDataString("-tag:sms_sent");

        var url =
            $"/admin/api/2024-04/orders.json" +
            $"?status=any" +
            $"&updated_at_min={startUtc:o}" +
            $"&updated_at_max={endUtc:o}" +
            $"&query={query}" +
            $"&limit=250";

        _logger.LogInformation(
            "📦 Shopify fulfilled orders fetch (SMS NOT SENT) → {Start} - {End}",
            startUtc, endUtc);

        var resp = await _httpClient.GetAsync(url, ct);
        resp.EnsureSuccessStatusCode();

        var json = await resp.Content.ReadAsStringAsync(ct);
        var raw = JsonConvert.DeserializeObject<ShopifyOrderRawResponse>(json);

        if (raw?.Orders == null || raw.Orders.Count == 0)
            return new();

        var result = raw.Orders
            // ✅ sadece fulfilled
            .Where(o =>
                string.Equals(
                    o.FulfillmentStatus,
                    "fulfilled",
                    StringComparison.OrdinalIgnoreCase) &&
                o.Fulfillments != null &&
                o.Fulfillments.Any()
            )
            // ✅ fulfillment tarihi aralıkta mı
            .Where(o =>
                o.Fulfillments!.Any(f =>
                    f.CreatedAt >= startUtc &&
                    f.CreatedAt <= endUtc
                )
            )
            // ✅ internal modele map
            .Select(o => new ShippedOrder
            {
                OrderId = o.Id,
                OrderNumber = o.Name,
                Phone = o.Customer?.Phone,
                Trackings = o.Fulfillments!
                    .SelectMany(f =>
                        f.TrackingNumbers?.Select(t => new ShippedTracking
                        {
                            TrackingNumber = t,
                            Company = f.TrackingCompany,
                            TrackingUrl = f.TrackingUrl
                        }) ?? Enumerable.Empty<ShippedTracking>()
                    )
                    .DistinctBy(t => t.TrackingNumber)
                    .ToList()
            })
            // ✅ tracking yoksa SMS atılmaz
            .Where(o => o.Trackings.Any())
            .ToList();

        _logger.LogInformation(
            "✅ SMS gönderilmemiş fulfilled sipariş sayısı: {Count}",
            result.Count);

        return result;
    }

    /// <summary>
    /// Shopify siparişine tag ekler (idempotent).
    /// Örn: sms_sent
    /// </summary>
    public async Task AddOrderTagAsync(
        long orderId,
        string tag,
        CancellationToken ct)
    {
        var getResp = await _httpClient.GetAsync(
            $"/admin/api/2024-04/orders/{orderId}.json",
            ct);

        getResp.EnsureSuccessStatusCode();

        var json = await getResp.Content.ReadAsStringAsync(ct);
        dynamic wrapper = JsonConvert.DeserializeObject<dynamic>(json)!;

        string existingTags = wrapper.order.tags ?? "";

        if (existingTags.Contains(tag, StringComparison.OrdinalIgnoreCase))
            return; // ✅ zaten ekli → idempotent

        var newTags = string.IsNullOrWhiteSpace(existingTags)
            ? tag
            : $"{existingTags}, {tag}";

        var body = new
        {
            order = new
            {
                id = orderId,
                tags = newTags
            }
        };

        var content = new StringContent(
            JsonConvert.SerializeObject(body),
            Encoding.UTF8,
            "application/json");

        var resp = await _httpClient.PutAsync(
            $"/admin/api/2024-04/orders/{orderId}.json",
            content,
            ct);

        resp.EnsureSuccessStatusCode();

        _logger.LogInformation(
            "🏷️ Shopify order {OrderId} tagged with {Tag}",
            orderId,
            tag);
    }
}
