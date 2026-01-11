namespace Dekofar.HyperConnect.Integrations.Shopify.Constants
{
    /// <summary>
    /// Shopify sipariş notlarında sistem tarafından kullanılan özel flag'ler
    /// </summary>
    public static class ShopifySystemNotes
    {
        /// <summary>
        /// Manuel reset işlemi sonrası eklenen flag.
        /// Webhook bu flag'i görürse otomatik etiketleme yapmaz.
        /// </summary>
        public const string ResetFlag = "[SYSTEM_RESET]";
    }
}
