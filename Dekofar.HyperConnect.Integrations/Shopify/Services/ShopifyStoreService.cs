using Dekofar.HyperConnect.Application.Common.Interfaces;
using Dekofar.HyperConnect.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
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

        /// <summary>
        /// Shopify mağazasını ekler veya günceller
        /// </summary>
        public async Task UpsertAsync(
            string shopDomain,
            string accessToken,
            string scopes,
            CancellationToken ct = default)
        {
            var store = await _db.ShopifyStores
                .FirstOrDefaultAsync(x => x.ShopDomain == shopDomain, ct);

            if (store == null)
            {
                store = new ShopifyStore
                {
                    Id = Guid.NewGuid(),
                    ShopDomain = shopDomain,
                    AccessToken = accessToken,
                    Scopes = scopes,
                    InstalledAtUtc = DateTime.UtcNow
                };

                _db.ShopifyStores.Add(store);
            }
            else
            {
                store.AccessToken = accessToken;
                store.Scopes = scopes;
                store.InstalledAtUtc = DateTime.UtcNow;
            }

            await _db.SaveChangesAsync(ct);
        }
        public async Task<ShopifyStore?> GetByShopDomainAsync(
    string shopDomain,
    CancellationToken ct)
        {
            return await _db.ShopifyStores
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.ShopDomain == shopDomain, ct);
        }

        /// <summary>
        /// Aktif (son kurulan) Shopify mağazasını getirir
        /// </summary>
        public async Task<ShopifyStore?> GetLatestAsync(
            CancellationToken ct = default)
        {
            return await _db.ShopifyStores
                .OrderByDescending(x => x.InstalledAtUtc)
                .FirstOrDefaultAsync(ct);
        }
    }
}
