using Dekofar.HyperConnect.Integrations.NetGsm.Models.sms;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dekofar.HyperConnect.Integrations.NetGsm.Interfaces.sms
{
    public interface INetGsmSmsService
    {
        Task<List<SmsInboxResponse>> GetInboxMessagesAsync(SmsInboxRequest request);
        Task<SmsSendResponse> SendSmsAsync(SmsSendRequest request);
    }
}
