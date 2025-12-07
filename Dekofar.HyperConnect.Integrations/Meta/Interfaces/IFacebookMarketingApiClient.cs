using Dekofar.HyperConnect.Integrations.Meta.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dekofar.HyperConnect.Integrations.Meta.Interfaces
{
    public interface IFacebookMarketingApiClient
    {
        Task<IReadOnlyList<FacebookAdDto>> GetActiveAdsAsync(
            string adAccountId,
            string accessToken,
            CancellationToken cancellationToken = default);
    }
}