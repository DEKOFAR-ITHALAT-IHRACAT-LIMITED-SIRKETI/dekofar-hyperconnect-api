using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Dekofar.HyperConnect.Integrations.Kargo.Dhl.BulkQuery.Models;

namespace Dekofar.HyperConnect.Integrations.Kargo.Dhl.BulkQuery
{
    public interface IDeliveredShipmentService
    {
        Task<List<DeliveredShipmentResponse>> GetDeliveredShipmentsAsync(string startDate);
    }
}
