using Dekofar.HyperConnect.Application.Common.Interfaces;
using Dekofar.HyperConnect.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Dekofar.HyperConnect.Infrastructure.Services
{
    public class JobStatsService : IJobStatsService
    {
        private readonly IApplicationDbContext _db;

        public JobStatsService(IApplicationDbContext db)
        {
            _db = db;
        }

        /// <summary>
        /// Bugün için istatistik kaydını getirir, yoksa oluşturur (UTC bazlı).
        /// </summary>
        private async Task<JobStat> GetOrCreateTodayAsync(CancellationToken ct)
        {
            var today = DateTime.UtcNow.Date; // ✅ Local yerine UTC

            var stat = await _db.JobStats
                .FirstOrDefaultAsync(s => s.Date == today, ct);

            if (stat == null)
            {
                stat = new JobStat
                {
                    Date = today,          // ✅ UTC olarak kaydet
                    PaidMarked = 0,
                    CancelTagged = 0
                };

                await _db.JobStats.AddAsync(stat, ct);
                await _db.SaveChangesAsync(ct);
            }

            return stat;
        }

        public async Task IncrementPaidMarkedAsync(CancellationToken ct = default)
        {
            var stat = await GetOrCreateTodayAsync(ct);
            stat.PaidMarked++;
            await _db.SaveChangesAsync(ct);
        }

        public async Task IncrementCancelTaggedAsync(CancellationToken ct = default)
        {
            var stat = await GetOrCreateTodayAsync(ct);
            stat.CancelTagged++;
            await _db.SaveChangesAsync(ct);
        }

        public async Task<JobStat?> GetTodayStatsAsync(CancellationToken ct = default)
        {
            var today = DateTime.UtcNow.Date; // ✅
            return await _db.JobStats
                .FirstOrDefaultAsync(s => s.Date == today, ct);
        }

        public async Task<List<JobStat>> GetStatsHistoryAsync(int days = 30, CancellationToken ct = default)
        {
            var fromDate = DateTime.UtcNow.Date.AddDays(-days); // ✅

            return await _db.JobStats
                .Where(s => s.Date >= fromDate)
                .OrderByDescending(s => s.Date)
                .ToListAsync(ct);
        }
    }
}
