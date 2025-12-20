using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dekofar.HyperConnect.Integrations.Kargo.Ptt.Auth
{
    public class PttAuthService : IPttAuthService
    {
        private readonly IConfiguration _configuration;

        public PttAuthService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public PttAuthCredentials GetCredentials()
        {
            return new PttAuthCredentials
            {
                CustomerId = _configuration["PttSettings:MusteriId"]
                             ?? throw new InvalidOperationException("PTT CustomerId not configured"),
                Username = _configuration["PttSettings:Kullanici"] ?? "PttWs",
                Password = _configuration["PttSettings:Sifre"]
                             ?? throw new InvalidOperationException("PTT Password not configured")
            };
        }
    }
}
