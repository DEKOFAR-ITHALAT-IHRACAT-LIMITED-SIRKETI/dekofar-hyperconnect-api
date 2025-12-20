using Dekofar.HyperConnect.Integrations.Kargo.Dhl.StandardQuery.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dekofar.HyperConnect.Integrations.Kargo.Dhl.StandardQuery.Interfaces
{
    public interface ITrackShipmentByReferenceIdService
    {
        Task<List<TrackShipmentResponse>> TrackShipmentByReferenceIdAsync(string referenceId);
    }
}
