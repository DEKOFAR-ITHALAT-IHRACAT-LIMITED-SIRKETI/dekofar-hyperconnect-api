using Dekofar.HyperConnect.Integrations.Shopify.Models.Internal;

namespace Dekofar.HyperConnect.Integrations.Sms.Templates
{
    public class SmsTemplateResolver : ISmsTemplateResolver
    {
        public string BuildMessage(
            ShippedOrder order,
            ShippedTracking tracking)
        {
            var company = tracking.Company?.ToLowerInvariant() ?? "";

            if (company.Contains("dhl"))
                return BuildDhl(tracking);

            if (company.Contains("ptt"))
                return BuildPtt(tracking);

            return BuildDefault(tracking);
        }

        /// <summary>
        /// ✅ DHL – kısa ve tek SMS
        /// </summary>
        private static string BuildDhl(ShippedTracking t)
        {
            return
$@"Siparisiniz kargoya verildi.

Kargo: DHL
Takip No: {t.TrackingNumber}
{t.TrackingUrl ?? "https://www.dhlecommerce.com.tr/gonderitakip"}

Siparisiniz icin tesekkur ederiz.
Dekofar";
        }

        /// <summary>
        /// ✅ PTT – kısa ve tek SMS
        /// </summary>
        private static string BuildPtt(ShippedTracking t)
        {
            return
$@"Siparisiniz kargoya verildi.

Kargo: PTT
Takip No: {t.TrackingNumber}
{t.TrackingUrl ?? "https://gonderitakip.ptt.gov.tr/"}

Siparisiniz icin tesekkur ederiz.
Dekofar";
        }

        /// <summary>
        /// ✅ Diğer kargolar
        /// </summary>
        private static string BuildDefault(ShippedTracking t)
        {
            return
$@"Siparisiniz kargoya verildi.

Takip No: {t.TrackingNumber}
{t.TrackingUrl ?? "-"}

Siparisiniz icin tesekkur ederiz.
Dekofar";
        }
    }
}
