using Dekofar.API.Hubs;
using Dekofar.API.Services;
using Dekofar.HyperConnect.API.Authorization;
using Dekofar.HyperConnect.Application;
using Dekofar.HyperConnect.Application.Common.Interfaces;
using Dekofar.HyperConnect.Application.Interfaces;
using Dekofar.HyperConnect.Application.Services;
using Dekofar.HyperConnect.Infrastructure.Jobs;
using Dekofar.HyperConnect.Infrastructure.ServiceRegistration;
using Dekofar.HyperConnect.Infrastructure.Services;
using Dekofar.HyperConnect.Integrations.Meta.Interfaces;
using Dekofar.HyperConnect.Integrations.Meta.Services;
using Dekofar.HyperConnect.Integrations.NetGsm.Models;
using Dekofar.HyperConnect.Integrations.NetGsm.Services.sms;
using Hangfire;
using Hangfire.MemoryStorage;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using System.IO.Compression;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

#region 🌐 CORS

const string CorsPolicyName = "_dekofarCors";

builder.Services.AddCors(options =>
{
    options.AddPolicy(CorsPolicyName, policy =>
    {
        policy.WithOrigins(
                "http://localhost:4200",
                "http://192.168.1.100:4200",
                "https://hyperconnect.dekofar.com",
                "https://dekofar-hyperconnect-api-production.up.railway.app"
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

#endregion

#region 📦 Core Services

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication();
builder.Services.AddMemoryCache();

#endregion

#region 🔐 Authorization

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("CanAssignTicket", p =>
        p.Requirements.Add(new PermissionRequirement("CanAssignTicket")));
    options.AddPolicy("CanManageDiscounts", p =>
        p.Requirements.Add(new PermissionRequirement("CanManageDiscounts")));
    options.AddPolicy("CanEditDueDate", p =>
        p.Requirements.Add(new PermissionRequirement("CanEditDueDate")));
});

builder.Services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();

#endregion

#region ⏰ Hangfire

builder.Services.AddHangfire(config =>
{
    config.UseMemoryStorage();
});

builder.Services.AddHangfireServer();

#endregion

#region 📬 Application Services

builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IModerationService, ModerationService>();

#endregion

#region 📡 Controllers / JSON

builder.Services
    .AddControllers()
    .AddNewtonsoftJson(opt =>
    {
        opt.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
        opt.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
    });

builder.Services.AddSignalR();

#endregion

#region 📲 NetGSM

builder.Services.Configure<NetGsmOptions>(
    builder.Configuration.GetSection("NetGsm"));

#endregion

#region 🧠 Cache & Compression

builder.Services.AddResponseCaching(opt =>
{
    opt.MaximumBodySize = 5 * 1024 * 1024;
    opt.UseCaseSensitivePaths = false;
});

builder.Services.AddResponseCompression(opt =>
{
    opt.EnableForHttps = true;
    opt.MimeTypes = ResponseCompressionDefaults.MimeTypes
        .Concat(new[] { "application/json" });
});

builder.Services.Configure<GzipCompressionProviderOptions>(o =>
    o.Level = CompressionLevel.Fastest);

builder.Services.Configure<BrotliCompressionProviderOptions>(o =>
    o.Level = CompressionLevel.Fastest);

#endregion

#region 🌍 Http Clients

builder.Services.AddHttpClient();

builder.Services.AddHttpClient<IFacebookMarketingApiClient, FacebookMarketingApiClient>(c =>
{
    c.BaseAddress = new Uri("https://graph.facebook.com/v20.0/");
});

#endregion

#region 📘 Swagger

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1",
        new OpenApiInfo { Title = "Dekofar API", Version = "v1" });

    var jwtScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Bearer {token}",
        Reference = new OpenApiReference
        {
            Id = "Bearer",
            Type = ReferenceType.SecurityScheme
        }
    };

    c.AddSecurityDefinition("Bearer", jwtScheme);
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { jwtScheme, Array.Empty<string>() }
    });

    c.CustomSchemaIds(t => t.FullName);

    var xml = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xml);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath, true);
    }
});

#endregion

#region 🛰️ Forwarded Headers (Railway)

builder.Services.Configure<ForwardedHeadersOptions>(opt =>
{
    opt.ForwardedHeaders =
        ForwardedHeaders.XForwardedFor |
        ForwardedHeaders.XForwardedProto;

    opt.KnownNetworks.Clear();
    opt.KnownProxies.Clear();
});

#endregion

#region 📋 Logging

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

#endregion

var app = builder.Build();

#region 🧱 Middleware Pipeline

app.UseForwardedHeaders();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Dekofar API v1");
    c.RoutePrefix = "swagger";
});

app.UseResponseCompression();

app.UseCors(CorsPolicyName);

app.UseHttpsRedirection();

app.UseResponseCaching();

app.UseAuthentication();
app.UseAuthorization();

app.UseHangfireDashboard();

#endregion

#region 🗺️ Endpoints

app.MapControllers();
app.MapHub<ChatHub>("/hubs/chat");
app.MapHub<NotificationHub>("/hubs/notifications");
app.MapHub<SupportHub>("/supportHub");

app.MapGet("/", () => Results.Redirect("/swagger"));
app.MapGet("/health", () =>
    Results.Ok(new { ok = true, time = DateTime.UtcNow }));

#endregion

app.Run();
