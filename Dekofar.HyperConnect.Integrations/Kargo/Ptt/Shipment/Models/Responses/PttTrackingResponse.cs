namespace Dekofar.HyperConnect.Integrations.Kargo.Ptt.Tracking.Models.Responses
{
    public class PttTrackingResponse
    {
        public int HataKodu { get; set; }
        public string Aciklama { get; set; } = string.Empty;
        public List<PttTrackingItem> Items { get; set; } = new();
    }

    public class PttTrackingItem
    {
        public string Barkod { get; set; } = string.Empty;
        public string Durum { get; set; } = string.Empty;
        public string IslemAdi { get; set; } = string.Empty;
        public string IslemYeri { get; set; } = string.Empty;
        public DateTime? IslemTarihi { get; set; }
    }
}

