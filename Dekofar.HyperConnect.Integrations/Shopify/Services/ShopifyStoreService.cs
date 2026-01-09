using Dekofar.HyperConnect.Application.Common.Interfaces;
using Dekofar.HyperConnect.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
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

        // =====================================================
        // ➕ / 🔁 Shopify mağazasını ekler veya günceller
        // =====================================================
        public async Task UpsertAsync(
            string shopDomain,
            string accessToken,
            string scopes,
            CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(shopDomain))
                throw new ArgumentException("shopDomain is required");

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
                    InstalledAtUtc = DateTime.UtcNow,
                    IsActive = true
                };

                _db.ShopifyStores.Add(store);
            }
            else
            {
                store.AccessToken = accessToken;
                store.Scopes = scopes;
                store.InstalledAtUtc = DateTime.UtcNow;
                store.IsActive = true;
            }

            await _db.SaveChangesAsync(ct);
        }

        // =====================================================
        // 🔍 Shop domain’e göre mağaza getirir
        // =====================================================
        public async Task<ShopifyStore?> GetByShopDomainAsync(
            string shopDomain,
            CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(shopDomain))
                return null;

            return await _db.ShopifyStores
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.ShopDomain == shopDomain, ct);
        }

        // =====================================================
        // 🕒 En son kurulan (aktif) mağazayı getirir
        // =====================================================
        public async Task<ShopifyStore?> GetLatestAsync(
            CancellationToken ct = default)
        {
            return await _db.ShopifyStores
                .AsNoTracking()
                .Where(x => x.IsActive)
                .OrderByDescending(x => x.InstalledAtUtc)
                .FirstOrDefaultAsync(ct);
        }

        // =====================================================
        // ❌ Mağazayı pasif yap (uninstall webhook için)
        // =====================================================
        public async Task DeactivateAsync(
            string shopDomain,
            CancellationToken ct = default)
        {
            var store = await _db.ShopifyStores
                .FirstOrDefaultAsync(x => x.ShopDomain == shopDomain, ct);

            if (store == null)
                return;

            store.IsActive = false;
            await _db.SaveChangesAsync(ct);
        }
    }
}
