using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dekofar.HyperConnect.Integrations.NetGsm.Models
{
    public class NetGsmOptions
    {
        public string Username { get; set; } = default!;
        public string Password { get; set; } = default!;
        public string SendSmsBaseUrl { get; set; } = default!;
        public string ReceiveSmsBaseUrl { get; set; } = default!;
        public string DefaultHeader { get; set; } = default!;
    }
}
