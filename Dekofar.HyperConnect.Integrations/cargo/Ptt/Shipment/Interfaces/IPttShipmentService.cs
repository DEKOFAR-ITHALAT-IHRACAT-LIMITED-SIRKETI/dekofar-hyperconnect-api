using Dekofar.HyperConnect.Integrations.Kargo.Ptt.Shipment.Models.Requests;
using Dekofar.HyperConnect.Integrations.Kargo.Ptt.Shipment.Models.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dekofar.HyperConnect.Integrations.Kargo.Ptt.Shipment.Interfaces
{
    public interface IPttShipmentService
    {
        Task<PttKabulResponse> AddShipmentsAsync(IEnumerable<PttShipmentRequest> shipments);
    }
}
