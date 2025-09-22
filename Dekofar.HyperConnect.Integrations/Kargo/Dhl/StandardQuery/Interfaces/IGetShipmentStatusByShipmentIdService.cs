using Dekofar.HyperConnect.Integrations.Kargo.Dhl.StandardQuery.Models;
using System.Threading.Tasks;

namespace Dekofar.HyperConnect.Integrations.Kargo.Dhl.StandardQuery.Interfaces
{
    public interface IGetShipmentStatusByShipmentIdService
    {
        Task<GetShipmentStatusResponse> GetShipmentStatusByShipmentIdAsync(string shipmentId);
    }
}
