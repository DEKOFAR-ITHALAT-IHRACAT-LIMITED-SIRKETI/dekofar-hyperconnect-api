using System.Threading;
using System.Threading.Tasks;

namespace Dekofar.HyperConnect.Infrastructure.Jobs
{
    /// <summary>
    /// Hangfire veya benzeri sistemlerde periyodik olarak çalıştırılacak job'ların ortak arayüzü.
    /// </summary>
    public interface IRecurringJob
    {
        /// <summary>
        /// Job’un çalıştırılması gereken metot.
        /// </summary>
        /// <param name="cancellationToken">İptal tokeni</param>
        Task RunAsync(CancellationToken cancellationToken = default);
    }
}
