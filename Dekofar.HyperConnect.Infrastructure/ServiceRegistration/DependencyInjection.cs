using Dekofar.HyperConnect.Application;
using Dekofar.HyperConnect.Application.Common.Interfaces;
using Dekofar.HyperConnect.Application.Interfaces;
using Dekofar.HyperConnect.Domain.Entities;
using Dekofar.HyperConnect.Infrastructure.Jobs;
using Dekofar.HyperConnect.Infrastructure.Persistence;
using Dekofar.HyperConnect.Infrastructure.Persistence.Repositories;
using Dekofar.HyperConnect.Infrastructure.Services;
using Dekofar.HyperConnect.Integrations.Kargo.Dhl.Auth.Interfaces;
using Dekofar.HyperConnect.Integrations.Kargo.Dhl.Auth.Services;
using Dekofar.HyperConnect.Integrations.Kargo.Dhl.BulkQuery;
using Dekofar.HyperConnect.Integrations.Kargo.Dhl.BulkQuery.Services;
using Dekofar.HyperConnect.Integrations.NetGsm.Interfaces;
using Dekofar.HyperConnect.Integrations.NetGsm.Services;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;


// ✅ Alias tanımı: sadece Bulk_Query altındaki interface kullanılacak

namespace Dekofar.HyperConnect.Infrastructure.ServiceRegistration
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // 📦 DbContext
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

            // 📦 IApplicationDbContext implementasyonu
            services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());

            // 🔐 Identity (ApplicationUser + Role<Guid>)
            services.AddIdentity<ApplicationUser, IdentityRole<Guid>>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 6;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();


            // 🔑 Alias ile doğru interface kullanımı
            services.AddScoped<IShipmentByDateService, ShipmentByDateService>();
            services.AddScoped<IDeliveredShipmentService, DeliveredShipmentService>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IStatusChangedShipmentService, StatusChangedShipmentService>();

            // 📦 Job Stats servisleri
            services.AddScoped<IJobStatsService, JobStatsService>();


            // 🔑 DHL servisleri
            services.AddScoped<IShipmentByDateService, ShipmentByDateService>();
            services.AddScoped<IDeliveredShipmentService, DeliveredShipmentService>();

            // 🔑 MNG servisleri

            // 🔐 Ortak Auth
            services.AddScoped<IAuthService, AuthService>();

            // 📦 Job Stats
            services.AddScoped<IJobStatsService, JobStatsService>();




            // 📦 Recurring Job (DHL → Shopify sync job)
            services.AddScoped<IRecurringJob, DhlShopifySyncJob>();
            services.AddScoped<DhlShopifySyncJob>(); // direkt job enjekte etmek için

            // JWT authentication Program.cs’de
            services.AddHttpContextAccessor();
            services.AddScoped<ICurrentUserService, CurrentUserService>();
            services.AddScoped<IFileStorageService, LocalFileStorageService>();
            services.AddScoped<IActivityLogger, ActivityLogger>();
            services.AddScoped<IUserNotificationService, UserNotificationService>();
            services.AddScoped<IBadgeService, BadgeService>();
            services.AddScoped<IWorkSessionService, WorkSessionService>();

            // 🌐 Genel repo & IP servisleri
            services.AddScoped(typeof(IRepository<>), typeof(GenericRepository<>));
            services.AddScoped<IAllowedAdminIpService, AllowedAdminIpService>();

            // 📞 NetGSM servisleri
            services.AddScoped<INetGsmCallService, NetGsmCallService>();
            services.AddScoped<INetGsmSmsService, NetGsmSmsService>();

            // 🔑 Token & kullanıcı servisleri
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<IUserService, UserService>();

            // ✅ MediatR
            services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssembly(typeof(Application.AssemblyReference).Assembly);
            });

            return services;
        }
    }
}
