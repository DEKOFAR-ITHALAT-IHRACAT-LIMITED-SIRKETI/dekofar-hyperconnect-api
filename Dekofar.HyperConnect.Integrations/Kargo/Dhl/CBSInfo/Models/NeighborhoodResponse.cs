using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dekofar.HyperConnect.Integrations.Kargo.Dhl.CBSInfo.Models
{
    public class NeighborhoodResponse
    {
        public string CityId { get; set; } = string.Empty;
        public string DisctrictId { get; set; } = string.Empty;   // dikkat: API spelling hatasıyla "disctrictId"
        public string Distirict { get; set; } = string.Empty;     // dikkat: API spelling hatasıyla "distirict"
        public string Neighborhood { get; set; } = string.Empty;
    }
}
