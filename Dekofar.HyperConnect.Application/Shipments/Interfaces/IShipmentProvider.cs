using Dekofar.HyperConnect.Application.Shipments.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dekofar.HyperConnect.Application.Shipments.Interfaces
{
    public interface IShipmentProvider
    {
        Task<CreateShipmentResult> CreateAsync(CreateShipmentRequest request);
        Task CancelAsync(string referenceId);
        Task<TrackingResult> TrackAsync(string trackingNo);
    }
}
