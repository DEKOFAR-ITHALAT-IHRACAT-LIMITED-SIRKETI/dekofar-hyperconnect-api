using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dekofar.HyperConnect.Integrations.Kargo.Dhl.Auth.Models
{
    public class TokenRequest
    {
        public string CustomerNumber { get; set; }
        public string Password { get; set; }
        public int IdentityType { get; set; } = 1;
    }
}
