using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dekofar.HyperConnect.Application.Common.Options
{
    public class PttOptions
    {
        public string Environment { get; set; } = "Test";
        public string CustomerNumber { get; set; } = default!;
        public string Password { get; set; } = default!;
        public string PostCheckAccountNo { get; set; } = default!; // Kapıda ödeme
        public BarcodeRange Barcode { get; set; } = new();
        public PttEndpoints Endpoints { get; set; } = new();
    }

    public class BarcodeRange
    {
        public long Start { get; set; }
        public long End { get; set; }
    }

    public class PttEndpoints
    {
        public string Test { get; set; } = default!;
        public string Production { get; set; } = default!;
    }

}
