using Dekofar.HyperConnect.Domain.Entities;

namespace Dekofar.HyperConnect.Application.Common.Interfaces
{
    public interface IJobStatsService
    {
        Task IncrementPaidMarkedAsync(CancellationToken ct = default);
        Task IncrementCancelTaggedAsync(CancellationToken ct = default);
        Task<JobStat?> GetTodayStatsAsync(CancellationToken ct = default);
        Task<List<JobStat>> GetStatsHistoryAsync(int days = 30, CancellationToken ct = default);
    }
}
