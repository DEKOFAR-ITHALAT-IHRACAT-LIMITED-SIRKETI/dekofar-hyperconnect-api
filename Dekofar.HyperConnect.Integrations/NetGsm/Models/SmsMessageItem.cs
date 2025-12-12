using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dekofar.HyperConnect.Integrations.NetGsm.Models
{
    public class SmsMessageItem
    {
        public string Msg { get; set; } = string.Empty;
        public string No { get; set; } = string.Empty; // 905xxxxxxxxx
    }
}
