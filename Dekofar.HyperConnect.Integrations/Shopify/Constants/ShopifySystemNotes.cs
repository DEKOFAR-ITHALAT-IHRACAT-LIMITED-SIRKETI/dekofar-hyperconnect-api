namespace Dekofar.HyperConnect.Integrations.Shopify.Constants
{
    /// <summary>
    /// Shopify sistem notları için TEK kaynak
    /// </summary>
    public static class ShopifySystemNotes
    {
        /// <summary>
        /// Sistem tarafından yazılan tüm notların ortak prefix’i
        /// </summary>
        public const string SystemNotePrefix = "[SİSTEM]";

        /// <summary>
        /// Manuel reset sonrası webhook’un tekrar çalışmaması için flag
        /// </summary>
        public const string ResetFlag = "[SYSTEM_RESET]";
    }
}
