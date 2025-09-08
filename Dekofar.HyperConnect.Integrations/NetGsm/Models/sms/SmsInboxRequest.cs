using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dekofar.HyperConnect.Integrations.NetGsm.Models.sms
{
    /// <summary>
    /// Gelen kutusu sorgusu için istek modeli
    /// </summary>
    public class SmsInboxRequest
    {
        /// <summary>
        /// Başlangıç tarihi ("yyyy-MM-dd HH:mm")
        /// </summary>
        public string StartDate { get; set; }

        /// <summary>
        /// Bitiş tarihi ("yyyy-MM-dd HH:mm")
        /// </summary>
        public string StopDate { get; set; }
    }

}
