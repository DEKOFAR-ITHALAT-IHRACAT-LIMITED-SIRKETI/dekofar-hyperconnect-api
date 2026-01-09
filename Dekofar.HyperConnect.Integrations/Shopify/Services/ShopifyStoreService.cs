using Dekofar.HyperConnect.Application.Common.Interfaces;
using Dekofar.HyperConnect.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace Dekofar.HyperConnect.Application.Integrations.Shopify.Services
{
    public class ShopifyStoreService
    {
        private readonly IApplicationDbContext _db;

        public ShopifyStoreService(IApplicationDbContext db)
        {
            _db = db;
        }

        public async Task UpsertAsync(
            string shopDomain,
            string accessToken,
            string scopes)
        {
            var store = await _db.ShopifyStores
                .FirstOrDefaultAsync(x => x.ShopDomain == shopDomain);

            if (store == null)
            {
                store = new ShopifyStore
                {
                    Id = Guid.NewGuid(),
                    ShopDomain = shopDomain,
                    AccessToken = accessToken,
                    Scopes = scopes,
                    InstalledAt = DateTime.UtcNow,
                    IsActive = true
                };

                _db.ShopifyStores.Add(store);
            }
            else
            {
                store.AccessToken = accessToken;
                store.Scopes = scopes;
                store.IsActive = true;
                store.InstalledAt = DateTime.UtcNow;
            }

            await _db.SaveChangesAsync();
        }
    }
}
