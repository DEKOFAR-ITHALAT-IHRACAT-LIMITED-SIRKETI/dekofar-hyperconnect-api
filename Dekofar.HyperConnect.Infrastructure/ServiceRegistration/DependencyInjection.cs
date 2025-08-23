using Dekofar.HyperConnect.Application;
using Dekofar.HyperConnect.Application.Common.Interfaces;
using Dekofar.HyperConnect.Application.Interfaces;
using Dekofar.HyperConnect.Domain.Entities;
using Dekofar.HyperConnect.Infrastructure.Jobs;
using Dekofar.HyperConnect.Infrastructure.Persistence;
using Dekofar.HyperConnect.Infrastructure.Persistence.Repositories;
using Dekofar.HyperConnect.Infrastructure.Services;
using Dekofar.HyperConnect.Integrations.DHLKargo.Interfaces;
using Dekofar.HyperConnect.Integrations.DHLKargo.Services;
using Dekofar.HyperConnect.Integrations.NetGsm.Interfaces;
using Dekofar.HyperConnect.Integrations.NetGsm.Services;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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
            // 📦 MNG Kargo servisi
            services.AddScoped<IDhlKargoAuthService, DhlKargoAuthService>();


            // JWT authentication is configured in Program.cs to avoid duplicate scheme registration
            services.AddHttpContextAccessor(); // gerekli
            services.AddScoped<ICurrentUserService, CurrentUserService>();
            services.AddScoped<IFileStorageService, LocalFileStorageService>();
            services.AddScoped<IActivityLogger, ActivityLogger>();
            services.AddScoped<IUserNotificationService, UserNotificationService>();
            services.AddScoped<IBadgeService, BadgeService>();
            services.AddScoped<IWorkSessionService, WorkSessionService>();

            // 🌐 Genel depo ve IP servis kayıtları
            services.AddScoped(typeof(IRepository<>), typeof(GenericRepository<>));
            services.AddScoped<IAllowedAdminIpService, AllowedAdminIpService>();

            services.AddScoped<SupportTicketJobService>();

            // 📞 NetGSM servisleri
            services.AddScoped<INetGsmCallService, NetGsmCallService>();

            // 🔑 Token servisi
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
