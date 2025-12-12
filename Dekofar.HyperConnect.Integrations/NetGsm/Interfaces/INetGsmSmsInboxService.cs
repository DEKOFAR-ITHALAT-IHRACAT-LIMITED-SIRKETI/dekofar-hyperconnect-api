using Dekofar.HyperConnect.Integrations.NetGsm.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dekofar.HyperConnect.Integrations.NetGsm.Interfaces
{
    public interface INetGsmSmsInboxService
    {
        Task<List<SmsInboxResponse>> GetInboxAsync(SmsInboxRequest request);
    }
}
