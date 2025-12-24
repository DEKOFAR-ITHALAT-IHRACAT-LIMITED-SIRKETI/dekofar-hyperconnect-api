using Dekofar.HyperConnect.Application;
using Dekofar.HyperConnect.Application.Common.Interfaces;
using Dekofar.HyperConnect.Application.Interfaces;
using Dekofar.HyperConnect.Application.MediaDownloader.Interfaces;
using Dekofar.HyperConnect.Domain.Entities;
using Dekofar.HyperConnect.Infrastructure.Jobs;
using Dekofar.HyperConnect.Infrastructure.Persistence;
using Dekofar.HyperConnect.Infrastructure.Persistence.Repositories;
using Dekofar.HyperConnect.Infrastructure.Services;
using Dekofar.HyperConnect.Infrastructure.Services.MediaDownloader;
using Dekofar.HyperConnect.Integrations.Kargo.Dhl.Auth.Interfaces;
using Dekofar.HyperConnect.Integrations.Kargo.Dhl.Auth.Services;
using Dekofar.HyperConnect.Integrations.Kargo.Dhl.BulkQuery;
using Dekofar.HyperConnect.Integrations.Kargo.Dhl.BulkQuery.Interfaces;
using Dekofar.HyperConnect.Integrations.Kargo.Dhl.BulkQuery.Services;
using Dekofar.HyperConnect.Integrations.Kargo.Dhl.CBSInfo.Interfaces;
using Dekofar.HyperConnect.Integrations.Kargo.Dhl.CBSInfo.Services;
using Dekofar.HyperConnect.Integrations.Kargo.Dhl.StandardQuery.Interfaces;
using Dekofar.HyperConnect.Integrations.Kargo.Dhl.StandardQuery.Services;
using Dekofar.HyperConnect.Integrations.Kargo.Ptt.Auth;
using Dekofar.HyperConnect.Integrations.Kargo.Ptt.Shipment.Interfaces;
using Dekofar.HyperConnect.Integrations.Kargo.Ptt.Shipment.Services;
using Dekofar.HyperConnect.Integrations.Kargo.Ptt.Tracking.Interfaces;
using Dekofar.HyperConnect.Integrations.Kargo.Ptt.Tracking.Services;
using Dekofar.HyperConnect.Integrations.NetGsm.Interfaces;
using Dekofar.HyperConnect.Integrations.NetGsm.Services.sms;
using Dekofar.HyperConnect.Integrations.Shopify.Clients.GraphQl;
using Dekofar.HyperConnect.Integrations.Shopify.Common;
using Dekofar.HyperConnect.Integrations.Shopify.Customers.Services;
using Dekofar.HyperConnect.Integrations.Shopify.Fulfillment.Services;
using Dekofar.HyperConnect.Integrations.Shopify.Orders.Rules;
using Dekofar.HyperConnect.Integrations.Shopify.Orders.Services;
using Dekofar.HyperConnect.Integrations.Shopify.Products.Services;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http.Headers;

namespace Dekofar.HyperConnect.Infrastructure.ServiceRegistration
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // -------------------- DB --------------------
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

            services.AddScoped<IApplicationDbContext>(sp =>
                sp.GetRequiredService<ApplicationDbContext>());

            // -------------------- Identity --------------------
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

            // -------------------- DHL --------------------
            services.AddScoped<IShipmentByDateService, ShipmentByDateService>();
            services.AddScoped<IDeliveredShipmentService, DeliveredShipmentService>();
            services.AddScoped<IStatusChangedShipmentService, StatusChangedShipmentService>();
            services.AddScoped<IShipmentByDateDetailService, ShipmentByDateDetailService>();
            services.AddScoped<ICbsInfoService, CbsInfoService>();
            services.AddScoped<IAuthService, AuthService>();

            services.AddScoped<IGetOrderService, GetOrderService>();
            services.AddScoped<IGetShipmentService, GetShipmentService>();
            services.AddScoped<IGetShipmentByShipmentIdService, GetShipmentByShipmentIdService>();
            services.AddScoped<IGetShipmentStatusByReferenceIdService, GetShipmentStatusByReferenceIdService>();
            services.AddScoped<IGetShipmentStatusByShipmentIdService, GetShipmentStatusByShipmentIdService>();
            services.AddScoped<ITrackShipmentByReferenceIdService, TrackShipmentByReferenceIdService>();
            services.AddScoped<ITrackShipmentByShipmentIdService, TrackShipmentByShipmentIdService>();

            // 🔴 KRİTİK – EKSİK OLAN BUYDU
            // ⬆️ Eğer sınıf adı farklıysa (DhlRecurringJob vb.) onu yaz

            // -------------------- PTT --------------------
            services.AddScoped<IPttAuthService, PttAuthService>();
            services.AddHttpClient<IPttShipmentService, PttShipmentService>();
            services.AddHttpClient<IPttDeleteService, PttDeleteService>();
            services.AddHttpClient<IPttTrackingService, PttTrackingService>();

            // -------------------- Genel --------------------
            services.AddHttpContextAccessor();
            services.AddScoped<ICurrentUserService, CurrentUserService>();
            services.AddScoped<IFileStorageService, LocalFileStorageService>();
            services.AddScoped<IActivityLogger, ActivityLogger>();
            services.AddScoped<IUserNotificationService, UserNotificationService>();
            services.AddScoped<IBadgeService, BadgeService>();
            services.AddScoped<IWorkSessionService, WorkSessionService>();
            services.AddScoped(typeof(IRepository<>), typeof(GenericRepository<>));
            services.AddScoped<IAllowedAdminIpService, AllowedAdminIpService>();

            // -------------------- NetGSM --------------------
            services.AddScoped<INetGsmSmsSendService, NetGsmSmsSendService>();
            services.AddScoped<INetGsmSmsInboxService, NetGsmSmsInboxService>();

            // -------------------- Shopify Options --------------------
            services.Configure<ShopifyOptions>(
                configuration.GetSection("Shopify"));

            // -------------------- Shopify HTTP Client --------------------
            services.AddHttpClient<ShopifyGraphQlClient>((sp, client) =>
            {
                var cfg = sp.GetRequiredService<IConfiguration>();
                client.BaseAddress = new Uri(cfg["Shopify:BaseUrl"]!);
                client.DefaultRequestHeaders.Add(
                    "X-Shopify-Access-Token",
                    cfg["Shopify:AccessToken"]!);
                client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));
            });

            // -------------------- Shopify Core Services --------------------
            services.AddScoped<ShopifyOrderReportService>();
            services.AddScoped<ShopifyCustomerService>();
            services.AddScoped<ShopifyProductService>();
            services.AddScoped<ShopifyFulfillmentService>();

            // -------------------- ORDER AUTO TAG RULES --------------------
            services.AddScoped<IOrderTagRule, CancelKeywordRule>();
            services.AddScoped<IOrderTagRule, BranchKeywordRule>();
            services.AddScoped<IOrderTagRule, ShortAddressRule>();
            services.AddScoped<IOrderTagRule, MultiProductRule>();
            services.AddScoped<IOrderTagRule, HighAmountRule>();
            services.AddScoped<IOrderTagRule, RepeatCustomerRule>();
            services.AddScoped<IOrderTagRule, RepeatPhoneOrderRule>();
            services.AddScoped<IOrderTagRule, ShippingDecisionRule>();

            services.AddScoped<ShopifyOrderTagEngine>();
            services.AddScoped<ShopifyOrderAutoTagService>();
            services.AddScoped<ShopifyOrderReprocessService>();

            // -------------------- Auth / Token --------------------
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<IUserService, UserService>();

            // -------------------- Cache & Media --------------------
            services.AddMemoryCache();
            services.AddScoped<IMediaDownloaderService, MediaDownloaderService>();

            // -------------------- MediatR --------------------
            services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssembly(
                    typeof(Application.AssemblyReference).Assembly);
            });

            return services;
        }
    }
}
