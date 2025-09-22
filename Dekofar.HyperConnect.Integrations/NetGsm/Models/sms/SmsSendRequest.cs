namespace Dekofar.HyperConnect.Integrations.NetGsm.Models.sms
{
    /// <summary>
    /// SMS gönderim isteği
    /// </summary>
    public class SmsSendRequest
    {
        public string MsgHeader { get; set; } = string.Empty;
        public string StartDate { get; set; } = string.Empty;
        public string StopDate { get; set; } = string.Empty;
        public string AppName { get; set; } = string.Empty;
        public string IysFilter { get; set; } = string.Empty;
        public string PartnerCode { get; set; } = string.Empty;
        public string Encoding { get; set; } = "TR";
        public List<SmsMessageItem> Messages { get; set; } = new();

    }

    public class SmsMessageItem
    {
        public string Msg { get; set; } = string.Empty;
        public string No { get; set; } = string.Empty;
    }

    public class SmsSendResponse
    {
        public string Code { get; set; } = string.Empty;

        /// <summary>Başarılı oldu mu?</summary>
        public bool Success { get; set; }

        /// <summary>NetGSM’in döndürdüğü ham yanıt (örn: 00, 70, ERR_...)</summary>
        public string RawResponse { get; set; } = string.Empty;

        /// <summary>İşlem JobId (sadece başarılı gönderimlerde gelir)</summary>
        public string? JobId { get; set; }

        /// <summary>Açıklama</summary>
        public string? Description { get; set; }
    }
}
