using Dekofar.HyperConnect.Integrations.Shopify.Models.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dekofar.HyperConnect.Integrations.Shopify.UseCases.Sms
{
    public interface IPreviewShippedOrdersSmsUseCase
    {
        Task<List<SmsPreviewItem>> ExecuteAsync(
            DateTime startUtc,
            DateTime endUtc,
            CancellationToken ct = default);
    }
}
