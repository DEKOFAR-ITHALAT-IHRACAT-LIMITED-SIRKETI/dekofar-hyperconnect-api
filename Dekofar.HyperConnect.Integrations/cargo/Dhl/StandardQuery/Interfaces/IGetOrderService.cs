using Dekofar.HyperConnect.Integrations.Kargo.Dhl.StandardQuery.Models;
using System.Threading.Tasks;

namespace Dekofar.HyperConnect.Integrations.Kargo.Dhl.StandardQuery.Interfaces
{
    public interface IGetOrderService
    {
        Task<GetOrderResponse> GetOrderAsync(string referenceId);
    }
}
