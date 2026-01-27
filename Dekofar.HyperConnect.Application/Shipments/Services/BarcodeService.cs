using Dekofar.HyperConnect.Application.Shipments.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dekofar.HyperConnect.Application.Shipments.Services
{
    public class BarcodeService : IBarcodeService
    {
        private long _current = DateTime.UtcNow.Ticks;

        public Task<string> NextAsync()
        {
            _current++;
            return Task.FromResult(_current.ToString());
        }
    }
}
