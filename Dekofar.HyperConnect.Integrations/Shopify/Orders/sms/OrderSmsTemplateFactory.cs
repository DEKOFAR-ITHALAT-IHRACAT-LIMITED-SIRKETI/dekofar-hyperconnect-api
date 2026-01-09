using Dekofar.HyperConnect.Integrations.Shopify.Orders.Decisions;
using Dekofar.HyperConnect.Integrations.Shopify.Orders.sms;

namespace Dekofar.HyperConnect.Integrations.Shopify.Orders.Sms;

public static class OrderSmsTemplateFactory
{
    public static string? CreateMessage(OrderSmsDecision sms)
    {
        return sms.Decision switch
        {
            OrderDecision.Automatic => CreateAutomaticMessage(sms),
            OrderDecision.ApprovalNeeded => CreateApprovalNeededMessage(),
            OrderDecision.Cancelled => null, // şimdilik SMS yok
            _ => null
        };
    }

    // =====================================================
    // 🟢 OTOMATIK
    // =====================================================
    private static string CreateAutomaticMessage(OrderSmsDecision sms)
    {
        var carrierPart =
            string.IsNullOrWhiteSpace(sms.ShippingCarrier)
                ? "kargo firması"
                : sms.ShippingCarrier;

        return
$@"Dekofar: Siparişiniz başarıyla onaylanmıştır.
Kargonuz {carrierPart} ile en kısa sürede gönderilecektir.
Teşekkür ederiz.";
    }

    // =====================================================
    // 🟠 ONAY GEREKLI
    // =====================================================
    private static string CreateApprovalNeededMessage()
    {
        return
@"Dekofar: Siparişiniz alınmıştır.
Sipariş onayı için en kısa sürede sizinle iletişime geçilecektir.
Anlayışınız için teşekkür ederiz.";
    }
}
