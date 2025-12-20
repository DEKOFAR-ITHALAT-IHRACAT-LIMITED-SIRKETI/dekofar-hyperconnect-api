using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dekofar.HyperConnect.Integrations.Kargo.Ptt.Shipment.Models.Responses
{
    public class PttKabulResponse
    {
        public int HataKodu { get; set; }
        public string Aciklama { get; set; } = string.Empty;
        public string DosyaAdi { get; set; } = string.Empty; // 🔹 ekle

        public List<PttKabulItemResult> Items { get; set; } = new();
    }

    public class PttKabulItemResult
    {
        public string Barcode { get; set; } = string.Empty;
        public int DonguHataKodu { get; set; }
        public string DonguAciklama { get; set; } = string.Empty;
        public bool Success { get; set; }
    }

}
