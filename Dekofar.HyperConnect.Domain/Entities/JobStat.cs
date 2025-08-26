using System;

namespace Dekofar.HyperConnect.Domain.Entities
{
    public class JobStat
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>İstatistik tarihi (gün bazlı, yyyy-MM-dd formatında tutulur)</summary>
        public DateTime Date { get; set; }

        /// <summary>Bugün kaç sipariş ödenmiş olarak işaretlendi</summary>
        public int PaidMarked { get; set; }

        /// <summary>Bugün kaç sipariş iptal etiketlendi</summary>
        public int CancelTagged { get; set; }
    }
}
