using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dekofar.HyperConnect.Integrations.NetGsm.Models
{
    public class SmsSendResponse
    {
        public bool Success { get; set; }
        public string Code { get; set; } = string.Empty;
        public string RawResponse { get; set; } = string.Empty;
        public string? JobId { get; set; }
        public string? Description { get; set; }
    }
}
