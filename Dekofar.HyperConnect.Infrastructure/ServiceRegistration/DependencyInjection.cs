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
using Dekofar.HyperConnect.Integrations.Kargo.Dhl.BulkQuery.Interfaces;
using Dekofar.HyperConnect.Integrations.Kargo.Dhl.BulkQuery.Services;
using Dekofar.HyperConnect.Integrations.Kargo.Dhl.CBSInfo.Interfaces;
using Dekofar.HyperConnect.Integrations.Kargo.Dhl.CBSInfo.Services;
using Dekofar.HyperConnect.Integrations.Kargo.Dhl.StandardQuery.Interfaces;
using Dekofar.HyperConnect.Integrations.Kargo.Dhl.StandardQuery.Services;
using Dekofar.HyperConnect.Integrations.NetGsm.Interfaces.sms;
using Dekofar.HyperConnect.Integrations.NetGsm.Services.sms;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

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

            // 🔑 DHL servisleri
            services.AddScoped<IShipmentByDateService, ShipmentByDateService>();
            services.AddScoped<IDeliveredShipmentService, DeliveredShipmentService>();
            services.AddScoped<IStatusChangedShipmentService, StatusChangedShipmentService>();
            services.AddScoped<IShipmentByDateDetailService, ShipmentByDateDetailService>();
            services.AddScoped<ICbsInfoService, CbsInfoService>();
            services.AddScoped<IAuthService, AuthService>();

            // 📦 DHL StandardQuery servisleri
            services.AddScoped<IGetOrderService, GetOrderService>();
            services.AddScoped<IGetShipmentService, GetShipmentService>();
            services.AddScoped<IGetShipmentByShipmentIdService, GetShipmentByShipmentIdService>();
            services.AddScoped<IGetShipmentStatusByReferenceIdService, GetShipmentStatusByReferenceIdService>();
            services.AddScoped<IGetShipmentStatusByShipmentIdService, GetShipmentStatusByShipmentIdService>();
            services.AddScoped<ITrackShipmentByReferenceIdService, TrackShipmentByReferenceIdService>();
            services.AddScoped<ITrackShipmentByShipmentIdService, TrackShipmentByShipmentIdService>();

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
            services.AddTransient<INetGsmSmsService, NetGsmSmsInboxService>();
            services.AddTransient<INetGsmSmsService, NetGsmSmsSendService>();

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
