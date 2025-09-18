using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dekofar.HyperConnect.Integrations.Kargo.Ptt.Shipment.Models.Requests
{
    public class PttShipmentRequest
    {
        public string Barcode { get; set; } = string.Empty;
        public string AliciAdi { get; set; } = string.Empty;
        public string AliciAdres { get; set; } = string.Empty;
        public string AliciIl { get; set; } = string.Empty;
        public string AliciIlce { get; set; } = string.Empty;
        public string AliciTel { get; set; } = string.Empty;
        public decimal Agirlik { get; set; }
        public decimal Desi { get; set; }
        public decimal Ucret { get; set; }
        public string MusteriReferansNo { get; set; } = Guid.NewGuid().ToString("N");
    }

}
