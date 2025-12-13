using Dekofar.HyperConnect.Integrations.Shopify.Models.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dekofar.HyperConnect.Integrations.Sms.Templates
{
    public interface ISmsTemplateResolver
    {
        string BuildMessage(
            ShippedOrder order,
            ShippedTracking tracking);
    }
}
