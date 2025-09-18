using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dekofar.HyperConnect.Integrations.Kargo.Ptt.Auth
{
    public interface IPttAuthService
    {
        /// <summary>
        /// Returns PTT credentials (MüşteriId, Kullanıcı, Şifre) 
        /// from configuration.
        /// </summary>
        PttAuthCredentials GetCredentials();
    }

    public class PttAuthCredentials
    {
        public string CustomerId { get; set; } = string.Empty;
        public string Username { get; set; } = "PttWs"; // genelde sabit
        public string Password { get; set; } = string.Empty;
    }
}