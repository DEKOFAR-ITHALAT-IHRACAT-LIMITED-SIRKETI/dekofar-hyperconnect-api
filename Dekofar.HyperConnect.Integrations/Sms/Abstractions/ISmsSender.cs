using Dekofar.HyperConnect.Integrations.NetGsm.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dekofar.HyperConnect.Integrations.Sms.Abstractions
{
    public interface ISmsSender
    {
        Task<SmsSendResponse> SendAsync(string phone, string message, CancellationToken ct);

    }
}
