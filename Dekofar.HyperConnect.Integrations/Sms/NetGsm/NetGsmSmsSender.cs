using Dekofar.HyperConnect.Integrations.NetGsm.Interfaces;
using Dekofar.HyperConnect.Integrations.NetGsm.Models;
using Dekofar.HyperConnect.Integrations.Sms.Abstractions;

namespace Dekofar.HyperConnect.Integrations.Sms.NetGsm
{
    public class NetGsmSmsSender : ISmsSender
    {
        private readonly INetGsmSmsSendService _netGsm;

        public NetGsmSmsSender(INetGsmSmsSendService netGsm)
        {
            _netGsm = netGsm;
        }

        public async Task SendAsync(string phone, string message, CancellationToken ct)
        {
            await _netGsm.SendAsync(new SmsSendRequest
            {
                MsgHeader = null, // default header appsettings’ten gelir
                Messages = new List<SmsMessageItem>
                {
                    new SmsMessageItem
                    {
                        No = phone,
                        Msg = message
                    }
                }
            });
        }
    }
}
