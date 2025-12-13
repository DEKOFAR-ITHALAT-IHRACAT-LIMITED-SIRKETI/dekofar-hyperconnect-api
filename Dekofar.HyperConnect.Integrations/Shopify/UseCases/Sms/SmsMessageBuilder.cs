using Dekofar.HyperConnect.Integrations.Shopify.Models.Internal;

namespace Dekofar.HyperConnect.Integrations.Shopify.UseCases.Sms
{
    public static class SmsMessageBuilder
    {
        public static string Build(ShippedTracking tracking)
        {
            var company = tracking.Company?.ToLowerInvariant() ?? "";

            // 🟢 DHL
            if (company.Contains("dhl"))
            {
                return
$@"Siparişiniz kargoya verilmiştir.

Kargo Firması: DHL eCommerce Türkiye
Kargo Takip No: {tracking.TrackingNumber}
Takip Linki: {tracking.TrackingUrl ?? "https://www.dhlecommerce.com.tr/gonderitakip"}

Detaylı bilgi almak veya herhangi bir sorunuz varsa bizimle iletişime geçebilirsiniz:
0850 304 32 25 – Çağrı Merkezi (Mesai saatleri içinde)
0850 304 32 25 – WhatsApp Destek Hattı (7/24 aktif)

Bizi tercih ettiğiniz için teşekkür ederiz.
Dekofar.com";
            }

            // 🟡 PTT
            if (company.Contains("ptt"))
            {
                return
$@"Siparişiniz kargoya verilmiştir.

Kargo Firması: PTT Kargo
Kargo Takip No: {tracking.TrackingNumber}
Takip Linki: {tracking.TrackingUrl ?? "https://gonderitakip.ptt.gov.tr/"}

Detaylı bilgi almak veya herhangi bir sorunuz varsa bizimle iletişime geçebilirsiniz:
0850 304 32 25 – Çağrı Merkezi (Mesai saatleri içinde)
0850 304 32 25 – WhatsApp Destek Hattı (7/24 aktif)

Bizi tercih ettiğiniz için teşekkür ederiz.
Dekofar.com";
            }

            // 🔵 Fallback – bilinmeyen kargo
            return
$@"Siparişiniz kargoya verilmiştir.

Kargo Firması: {tracking.Company ?? "Bilinmiyor"}
Kargo Takip No: {tracking.TrackingNumber}
{(string.IsNullOrWhiteSpace(tracking.TrackingUrl) ? "" : $"Takip Linki: {tracking.TrackingUrl}")}

Bizi tercih ettiğiniz için teşekkür ederiz.
Dekofar.com";
        }
    }
}
