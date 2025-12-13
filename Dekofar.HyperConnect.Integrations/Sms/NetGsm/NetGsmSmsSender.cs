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

        public async Task<SmsSendResponse> SendAsync(
            string phone,
            string message,
            CancellationToken ct = default)
        {
            var request = new SmsSendRequest
            {
                MsgHeader = null, // ✅ DefaultHeader appsettings’ten gelir
                Messages = new List<SmsMessageItem>
                {
                    new SmsMessageItem
                    {
                        No = phone,
                        Msg = message
                    }
                }
            };

            // 🔥 ALT SERVİSTEN GELEN SONUCU AYNEN DÖN
            return await _netGsm.SendAsync(request);
        }
    }
}
