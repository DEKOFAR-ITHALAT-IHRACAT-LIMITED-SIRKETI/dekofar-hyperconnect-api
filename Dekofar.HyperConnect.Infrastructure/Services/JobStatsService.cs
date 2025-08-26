using Dekofar.HyperConnect.Application.Common.Interfaces;
using Dekofar.HyperConnect.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Dekofar.HyperConnect.Infrastructure.Services
{
    public class JobStatsService : IJobStatsService
    {
        public Task<List<JobStat>> GetStatsHistoryAsync(int days = 30, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task<JobStat?> GetTodayStatsAsync(CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task IncrementCancelTaggedAsync(CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task IncrementPaidMarkedAsync(CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }
    }
}
