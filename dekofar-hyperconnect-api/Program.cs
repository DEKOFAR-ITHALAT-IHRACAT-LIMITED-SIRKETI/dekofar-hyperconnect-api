using System.Text;
using System.IdentityModel.Tokens.Jwt;
using System.Reflection; // Needed for XML comments
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Dekofar.HyperConnect.Integrations.NetGsm.Interfaces;
using Dekofar.HyperConnect.Integrations.NetGsm.Services;
using Dekofar.HyperConnect.Integrations.Shopify.Interfaces;
using Dekofar.HyperConnect.Integrations.Shopify.Services;
using Dekofar.HyperConnect.Application; // Application servis kayıtları
using Dekofar.HyperConnect.Application.Common.Interfaces;
using Dekofar.HyperConnect.Infrastructure.Services;
using Dekofar.HyperConnect.API.Authorization;
using MediatR;
using Dekofar.HyperConnect.Infrastructure.ServiceRegistration;
using Hangfire;
using Hangfire.MemoryStorage;
using Dekofar.HyperConnect.Infrastructure.Jobs;
using Microsoft.AspNetCore.Authorization;
using Dekofar.API.Hubs;
using Dekofar.API.Services;
using Dekofar.HyperConnect.Application.Interfaces;
using Dekofar.HyperConnect.Application.Services;
using Dekofar.HyperConnect.Infrastructure.Seeders;
using Microsoft.Extensions.Configuration;

var builder = WebApplication.CreateBuilder(args);

// 🌐 CORS Politikası
// Angular için yerel adres ve production ortamı için ana domain izinleri
// Not: Uygulama Azure App Service'te host ediliyorsa, Azure Portal üzerinden de CORS ayarlarının yapılması gerekir
var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins, policy =>
    {
        policy.WithOrigins(
                "http://localhost:4200", // Angular uygulaması (geliştirme)
                "https://dekofar.com"    // Production domain
            )
            .AllowAnyHeader()   // Tüm header'lara izin ver
            .AllowAnyMethod()   // Tüm HTTP metodlarına izin ver
            .AllowCredentials(); // Kimlik bilgileri (cookies, auth header) gönderimine izin ver
    });
});

// 📦 Altyapı Servisleri (DbContext, Identity, JWT vs.)
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddMemoryCache();
builder.Services.AddApplication();

// Register authorization policies backed by our custom requirement
builder.Services.AddAuthorization(options =>
{
    // Users must have the CanAssignTicket permission to access protected endpoints
    options.AddPolicy("CanAssignTicket", policy =>
        policy.Requirements.Add(new PermissionRequirement("CanAssignTicket")));

    // Controls access to discount management endpoints
    options.AddPolicy("CanManageDiscounts", policy =>
        policy.Requirements.Add(new PermissionRequirement("CanManageDiscounts")));

    // Allows editing support ticket due dates
    options.AddPolicy("CanEditDueDate", policy =>
        policy.Requirements.Add(new PermissionRequirement("CanEditDueDate")));
});

// Authorization handler that checks permission assignments for the current user
builder.Services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();

builder.Services.AddHangfire(config =>
{
    config.UseMemoryStorage();
});
builder.Services.AddHangfireServer();

// 📬 Entegrasyon Servisleri
builder.Services.AddScoped<INetGsmSmsService, NetGsmSmsService>();
builder.Services.AddHttpClient<IShopifyService, ShopifyService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IModerationService, ModerationService>();

// 📡 Controller & JSON Ayarları
builder.Services.AddControllers()
    .AddNewtonsoftJson(options =>
    {
        options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
        options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
    });

builder.Services.AddSignalR();

// 📘 Swagger + JWT Destekli Dokümantasyon
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Dekofar API", Version = "v1" });

    var jwtSecurityScheme = new OpenApiSecurityScheme
    {
        Scheme = "bearer",
        BearerFormat = "JWT",
        Name = "JWT Authentication",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Description = "JWT Bearer Token için `Bearer {token}` formatında giriniz",
        Reference = new OpenApiReference
        {
            Id = "Bearer",
            Type = ReferenceType.SecurityScheme
        }
    };

    c.AddSecurityDefinition("Bearer", jwtSecurityScheme);
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { jwtSecurityScheme, Array.Empty<string>() }
    });

    // Swashbuckle options to handle complex schemas and parameter naming
    c.UseAllOfToExtendReferenceSchemas();
    c.DescribeAllParametersInCamelCase();

    // Include generated XML comments for better documentation
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
});


// 📋 Logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

var app = builder.Build();

// 🧪 Swagger Arayüzü (Tüm ortamlarda aktif)
if (app.Environment.IsDevelopment() || app.Environment.IsStaging() || app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Dekofar API v1");
        c.RoutePrefix = "swagger";
    });
}

// 🌐 Orta Katmanlar - Sıralama önemlidir
app.UseHttpsRedirection(); // HTTP -> HTTPS yönlendirme

app.UseRouting(); // Rotaları belirle

app.UseCors(MyAllowSpecificOrigins); // Global CORS politikası

app.UseAuthentication(); // Kimlik doğrulama middleware'i
app.UseAuthorization();  // Yetkilendirme kontrolü

app.UseHangfireDashboard(); // Hangfire izleme paneli

app.MapControllers(); // API controller'larını endpoint olarak ekle

// 💬 SignalR hub'ları (gerçek zamanlı iletişim için)
app.MapHub<ChatHub>("/hubs/chat");
app.MapHub<NotificationHub>("/hubs/notifications");
app.MapHub<SupportHub>("/supportHub");

// 🚀 Seed default roles and admin user
//await SeedData.SeedDefaultsAsync(app.Services);

var configuration = app.Services.GetRequiredService<IConfiguration>();
//var enableTestData = configuration.GetValue<bool>("EnableTestData");
//await TestDataSeeder.SeedAsync(app.Services, enableTestData);

using (var scope = app.Services.CreateScope())
{
    var recurringJobManager = scope.ServiceProvider.GetRequiredService<IRecurringJobManager>();

    recurringJobManager.AddOrUpdate<SupportTicketJobService>(
        "CloseStaleTickets",
        x => x.CloseOldTickets(),
        Cron.Daily);

    recurringJobManager.AddOrUpdate<SupportTicketJobService>(
        "NotifyUnassignedTickets",
        x => x.NotifyAdminOfUnassignedTickets(),
        Cron.Daily);
}

app.Run();
