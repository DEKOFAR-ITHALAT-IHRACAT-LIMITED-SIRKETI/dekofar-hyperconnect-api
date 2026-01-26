using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dekofar.HyperConnect.Integrations.Kargo.Ptt.Utils
{
    public class BarcodeService : IBarcodeService
    {
        public Task<string> NextAsync()
        {
            // SELECT ... FOR UPDATE
            // last + 1
            // range check
            throw new NotImplementedException();
        }
    }


}
