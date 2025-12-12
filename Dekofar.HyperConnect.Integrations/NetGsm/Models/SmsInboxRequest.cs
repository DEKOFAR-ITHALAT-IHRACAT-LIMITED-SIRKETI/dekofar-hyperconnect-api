using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dekofar.HyperConnect.Integrations.NetGsm.Models
{
    public class SmsInboxRequest
    {
        // yyyy-MM-dd HH:mm
        public string StartDate { get; set; } = string.Empty;
        public string StopDate { get; set; } = string.Empty;
    }
}
