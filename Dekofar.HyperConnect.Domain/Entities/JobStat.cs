using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dekofar.HyperConnect.Domain.Entities
{
    public class JobStat
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public int PaidMarked { get; set; }
        public int CancelTagged { get; set; }
    }
}
