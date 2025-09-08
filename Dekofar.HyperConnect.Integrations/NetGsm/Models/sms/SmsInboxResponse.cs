namespace Dekofar.HyperConnect.Integrations.NetGsm.Models.sms
{
    /// <summary>
    /// Gelen SMS satırları
    /// </summary>
    public class SmsInboxResponse
    {
        /// <summary>
        /// Gönderen numara (gsmno)
        /// </summary>
        public string Orjinator { get; set; }

        /// <summary>
        /// Mesaj içeriği
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Mesaj tarihi (NetGSM formatı veya "yyyy-MM-dd HH:mm:ss")
        /// </summary>
        public string Date { get; set; }
    }
}
