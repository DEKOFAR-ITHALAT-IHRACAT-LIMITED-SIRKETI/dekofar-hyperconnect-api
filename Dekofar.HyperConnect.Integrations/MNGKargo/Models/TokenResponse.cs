using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dekofar.HyperConnect.Integrations.MNGKargo.Models
{
    public class TokenResponse
    {
        public string jwt { get; set; }
        public string refreshToken { get; set; }
        public string jwtExpireDate { get; set; }
        public string refreshTokenExpireDate { get; set; }
    }




}
