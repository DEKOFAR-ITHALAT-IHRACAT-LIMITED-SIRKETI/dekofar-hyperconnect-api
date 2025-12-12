using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dekofar.HyperConnect.Integrations.NetGsm.Models
{
    public class SmsSendRequest
    {
        public string MsgHeader { get; set; } = string.Empty;
        public List<SmsMessageItem> Messages { get; set; } = new();
    }
}
