using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dekofar.HyperConnect.Integrations.Kargo.Dhl.Models;

namespace Dekofar.HyperConnect.Integrations.Kargo.Dhl.Interfaces
{
    public interface IDhlKargoDeliveredShipmentService
    {
        /// <summary>
        /// Belirtilen tarihte teslim edilen gönderileri getirir (dd-MM-yyyy formatında).
        /// </summary>
        Task<List<DeliveredShipmentResponse>> GetDeliveredShipmentsByDateAsync(DateTime startDate);
    }
}
