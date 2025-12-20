using Dekofar.HyperConnect.Integrations.Kargo.Dhl.StandardQuery.Models;
using System.Threading.Tasks;

namespace Dekofar.HyperConnect.Integrations.Kargo.Dhl.StandardQuery.Interfaces
{
    public interface IGetShipmentStatusByReferenceIdService
    {
        Task<GetShipmentStatusResponse> GetShipmentStatusByReferenceIdAsync(string referenceId);
    }
}
