using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dekofar.HyperConnect.Integrations.Kargo.Ptt.Utils
{
    public interface IBarcodeService
    {
        Task<string> NextAsync();
    }
}
