using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dekofar.HyperConnect.Integrations.Meta.Models
{
    public class FacebookAdDto
    {
        public string Id { get; set; } = default!;
        public string Name { get; set; } = default!;
        public string EffectiveStatus { get; set; } = default!;
        public string? PostId { get; set; }
    }
}
