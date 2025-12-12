namespace Dekofar.HyperConnect.Integrations.Shopify.Models.Internal
{
    /// <summary>
    /// Shopify'dan gönderilmiþ (fulfilled) sipariþlerin
    /// sistem içinde kullanýlan sade modeli
    /// </summary>
    public class ShippedOrder
    {
        public long OrderId { get; set; }
        public string? OrderNumber { get; set; }
        public string? Phone { get; set; }

        /// <summary>
        /// Kargo takip numaralarý (DHL, PTT vb.)
        /// </summary>
        public List<string> TrackingNumbers { get; set; } = new();
    }
}
