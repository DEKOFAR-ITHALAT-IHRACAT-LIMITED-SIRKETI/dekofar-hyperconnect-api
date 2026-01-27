using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Dekofar.HyperConnect.Infrastructure.Jobs
{
    /// <summary>
    /// Fallback implementation used when a concrete recurring job is not wired.
    /// Keeps DI and manual trigger endpoints from failing at startup.
    /// </summary>
    public class NoOpRecurringJob : IRecurringJob
    {
        private readonly ILogger<NoOpRecurringJob> _logger;

        public NoOpRecurringJob(ILogger<NoOpRecurringJob> logger)
        {
            _logger = logger;
        }

        public Task RunAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogWarning(
                "IRecurringJob is not configured with a concrete implementation. " +
                "No operation was performed.");
            return Task.CompletedTask;
        }
    }
}
