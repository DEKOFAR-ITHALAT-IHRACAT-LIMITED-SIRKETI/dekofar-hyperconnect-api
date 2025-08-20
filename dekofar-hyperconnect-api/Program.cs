using Dekofar.API.Hubs;
using Dekofar.API.Services;
using Dekofar.HyperConnect.API.Authorization;
using Dekofar.HyperConnect.Application; // Application servis kayıtları
using Dekofar.HyperConnect.Application.Common.Interfaces;
using Dekofar.HyperConnect.Application.Interfaces;
using Dekofar.HyperConnect.Application.Services;
using Dekofar.HyperConnect.Infrastructure.Jobs;
using Dekofar.HyperConnect.Infrastructure.ServiceRegistration;
using Dekofar.HyperConnect.Infrastructure.Services;
using Dekofar.HyperConnect.Integrations.NetGsm.Interfaces;
using Dekofar.HyperConnect.Integrations.NetGsm.Services;
using Dekofar.HyperConnect.Integrations.Shopify.Interfaces;
using Dekofar.HyperConnect.Integrations.Shopify.Services;
using Hangfire;
using Hangfire.MemoryStorage;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Reflection;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

//
// 🌐 CORS Politikası
//
var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins, policy =>
    {
        policy.WithOrigins(
                "http://localhost:4200",
                "http://192.168.1.100:4200",
                "https://hyperconnect.dekofar.com"
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

//
// 📦 Altyapı Servisleri (DbContext, Identity, JWT vs.)
//
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddMemoryCache();
builder.Services.AddApplication();

//
// 🔐 Yetkilendirme politikaları
//
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("CanAssignTicket", policy =>
        policy.Requirements.Add(new PermissionRequirement("CanAssignTicket")));
    options.AddPolicy("CanManageDiscounts", policy =>
        policy.Requirements.Add(new PermissionRequirement("CanManageDiscounts")));
    options.AddPolicy("CanEditDueDate", policy =>
        policy.Requirements.Add(new PermissionRequirement("CanEditDueDate")));
});
builder.Services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();

//
// ⏰ Hangfire (in-memory)
//
builder.Services.AddHangfire(config => { config.UseMemoryStorage(); });
builder.Services.AddHangfireServer();

//
// 📬 Entegrasyon Servisleri
//
builder.Services.AddScoped<INetGsmSmsService, NetGsmSmsService>();
builder.Services.AddHttpClient<IShopifyService, ShopifyService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IModerationService, ModerationService>();

//
// 📡 Controllers & JSON
//
builder.Services.AddControllers()
    .AddNewtonsoftJson(options =>
    {
        options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
        options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
    });

builder.Services.AddSignalR();

//
// 📘 Swagger + JWT
//
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
        Reference = new OpenApiReference { Id = "Bearer", Type = ReferenceType.SecurityScheme }
    };

    c.AddSecurityDefinition("Bearer", jwtSecurityScheme);
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { jwtSecurityScheme, Array.Empty<string>() }
    });

    // opsiyonel ayarlar
    c.UseAllOfToExtendReferenceSchemas();
    c.DescribeAllParametersInCamelCase();

    // XML yorumlarını yalnızca dosya varsa ekle (prod'da güvenli)
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
    }
});

//
// 📋 Logging
//
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

//
// 🛰️ Proxy/Forwarded Headers (Railway için gerekli)
//
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

var app = builder.Build();

//
// 🧭 Forwarded Headers en başta
//
app.UseForwardedHeaders();

//
// 🧪 Swagger (tüm ortamlarda aktif kalsın)
//
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Dekofar API v1");
    c.RoutePrefix = "swagger";
});

//
// 🌐 Orta Katmanlar
//
app.UseCors(MyAllowSpecificOrigins);
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

//
// ⛏️ Hangfire Dashboard (gerekirse auth ekleyebilirsin)
//
app.UseHangfireDashboard();

//
// 🗺️ Endpointler
//
app.MapControllers();
app.MapHub<ChatHub>("/hubs/chat");
app.MapHub<NotificationHub>("/hubs/notifications");
app.MapHub<SupportHub>("/supportHub");

// Kök path'i Swagger'a yönlendir
app.MapGet("/", () => Results.Redirect("/swagger"));

// Basit health endpoint
app.MapGet("/health", () => Results.Ok(new { ok = true, time = DateTime.UtcNow }));

//
// ⏱️ Recurring Jobs
//

//
// 🚀 Run
//
app.Run();
