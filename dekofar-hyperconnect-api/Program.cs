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
using Dekofar.HyperConnect.Integrations.Shopify.Interfaces;
using Dekofar.HyperConnect.Integrations.Shopify.Services;
using Hangfire;
using Hangfire.MemoryStorage;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.ResponseCompression;   // ✅ Response Compression
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using System.IO.Compression;                      // ✅ Compression level
using System.IdentityModel.Tokens.Jwt;
using System.Net;                                 // ✅ DecompressionMethods
using System.Net.Http;                            // ✅ SocketsHttpHandler
using System.Reflection;
using System.Security.Claims;
using System.Text;
using Dekofar.HyperConnect.Integrations.NetGsm.Interfaces.sms;
using Dekofar.HyperConnect.Integrations.NetGsm.Services.sms;

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
builder.Services.AddScoped(typeof(INotificationService), typeof(NotificationService));
builder.Services.AddScoped(typeof(IDashboardService), typeof(DashboardService));
builder.Services.AddScoped(typeof(IModerationService), typeof(ModerationService));

// ✅ Shopify HttpClient: otomatik gzip/deflate, connection pooling (servis içinde header’lar zaten set ediliyor)
builder.Services.AddHttpClient<IShopifyService, ShopifyService>()
    .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
    {
        AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
        PooledConnectionLifetime = TimeSpan.FromMinutes(10),
        PooledConnectionIdleTimeout = TimeSpan.FromMinutes(5),
        MaxConnectionsPerServer = 10
    });

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
// 🧠 Response Caching & Compression (🔑 Controller’daki [ResponseCache] için gerekli)
//
builder.Services.AddResponseCaching(options =>
{
    // Büyük JSON cevaplar için üst sınır; gerekirse arttırın
    options.MaximumBodySize = 5 * 1024 * 1024; // 5 MB
    options.UseCaseSensitivePaths = false;
});

builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    // JSON’u da sıkıştır (Swagger, HTML vb. zaten default listede)
    options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[] { "application/json" });
});
builder.Services.Configure<BrotliCompressionProviderOptions>(o => o.Level = CompressionLevel.Fastest);
builder.Services.Configure<GzipCompressionProviderOptions>(o => o.Level = CompressionLevel.Fastest);

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

    // 🔑 ÇAKIŞMALARI ENGELLEMEK İÇİN
    c.CustomSchemaIds(type => type.FullName);

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
// 🔽 Response Compression erken devreye
//
app.UseResponseCompression();

//
// 🌐 Orta Katmanlar
//
app.UseCors(MyAllowSpecificOrigins);
app.UseHttpsRedirection();

// ✅ Response Caching (Controller’daki [ResponseCache] ile çalışır)
app.UseResponseCaching();

app.UseAuthentication();
app.UseAuthorization();

//
// ⛏️ Hangfire Dashboard
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

////
// ⏱️ Recurring Jobs (örnek)
// RecurringJob.AddOrUpdate<DhlShopifySyncJob>(
//     "dhl-shopify-sync",
//     job => job.RunAsync(CancellationToken.None),
//     "*/5 * * * *"   // her 5 dakikada bir (test için)
//// );

//
// 🚀 Run
//
app.Run();
