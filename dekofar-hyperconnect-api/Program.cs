using Dekofar.API.Hubs;
using Dekofar.API.Services;
using Dekofar.HyperConnect.API.Authorization;
using Dekofar.HyperConnect.Application;
using Dekofar.HyperConnect.Application.Common.Interfaces;
using Dekofar.HyperConnect.Application.Interfaces;
using Dekofar.HyperConnect.Application.Services;
using Dekofar.HyperConnect.Infrastructure.Jobs;
using Dekofar.HyperConnect.Infrastructure.Persistence;
using Dekofar.HyperConnect.Infrastructure.ServiceRegistration;
using Dekofar.HyperConnect.Infrastructure.Services;
using Dekofar.HyperConnect.Integrations.NetGsm.Interfaces.sms;
using Dekofar.HyperConnect.Integrations.NetGsm.Services.sms;
using Dekofar.HyperConnect.Integrations.Shopify.Common;
using Dekofar.HyperConnect.Integrations.Shopify.OAuth;
using Hangfire;
using Hangfire.MemoryStorage;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using System.IO.Compression;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

//
// 🌐 CORS
//
const string MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
builder.Services.AddCors(options =>
{
    options.AddPolicy(MyAllowSpecificOrigins, policy =>
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
// 📦 Core Katmanlar
//
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication();
builder.Services.AddMemoryCache();

//
// 🔐 Authorization
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
// ⏰ Hangfire
//
builder.Services.AddHangfire(config => config.UseMemoryStorage());
builder.Services.AddHangfireServer();

//
// 📬 Application Servisleri
//
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IModerationService, ModerationService>();

//
// 🛒 Shopify OAuth
//
builder.Services.Configure<ShopifyOptions>(
    builder.Configuration.GetSection("Shopify"));
builder.Services.AddScoped<ShopifyOAuthService>();

//
// 📡 Controllers
//
builder.Services.AddControllers()
    .AddNewtonsoftJson(options =>
    {
        options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
        options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
    });

builder.Services.AddSignalR();

//
// 🧠 Response Cache & Compression
//
builder.Services.AddResponseCaching();

builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.MimeTypes = ResponseCompressionDefaults.MimeTypes
        .Concat(new[] { "application/json" });
});
builder.Services.Configure<GzipCompressionProviderOptions>(o =>
    o.Level = CompressionLevel.Fastest);

//
// 📘 Swagger
//
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Dekofar API",
        Version = "v1"
    });

    c.CustomSchemaIds(type => type.FullName);

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
        c.IncludeXmlComments(xmlPath);
});

//
// 🛰️ Forwarded Headers (Railway)
//
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders =
        ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

var app = builder.Build();

//
// 🧱 AUTO MIGRATION (PROD SAFE)
//
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
}

//
// 🧭 Middleware
//
app.UseForwardedHeaders();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Dekofar API v1");
    c.RoutePrefix = "swagger";
});

app.UseResponseCompression();
app.UseCors(MyAllowSpecificOrigins);
app.UseHttpsRedirection();
app.UseResponseCaching();

app.UseAuthentication();
app.UseAuthorization();

app.UseHangfireDashboard();

//
// 🗺️ Endpoints
//
app.MapControllers();
app.MapHub<ChatHub>("/hubs/chat");
app.MapHub<NotificationHub>("/hubs/notifications");
app.MapHub<SupportHub>("/supportHub");

app.MapGet("/", () => Results.Redirect("/swagger"));
app.MapGet("/health", () => Results.Ok(new { ok = true, time = DateTime.UtcNow }));

app.Run();
