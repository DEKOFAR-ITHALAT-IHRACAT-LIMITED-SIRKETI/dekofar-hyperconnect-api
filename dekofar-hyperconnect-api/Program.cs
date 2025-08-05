using System.Text;
using System.IdentityModel.Tokens.Jwt;
using System.Reflection; // Needed for XML comments
using System.Security.Claims;
using System.Threading.Tasks;
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
using Microsoft.AspNetCore.Http; // For returning custom status codes
using Npgsql; // PostgreSQL bağlantı testleri için

var builder = WebApplication.CreateBuilder(args);

// Ensure incoming JWT tokens keep original claim types (e.g. "sub", "role")
JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

// Configure JWT authentication only once to avoid duplicate 'Bearer' scheme registration
// that previously caused "Scheme already exists: Bearer" on startup. The Infrastructure
// layer no longer registers authentication so all JWT settings live here.
var jwtSettings = builder.Configuration.GetSection("Jwt");
builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        // Do not require HTTPS metadata since tokens may come from different hosts
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]!)),
            ClockSkew = TimeSpan.Zero,
            RoleClaimType = ClaimTypes.Role
        };

        // Allow JWT tokens to be passed via query string for SignalR hubs
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) &&
                    (path.StartsWithSegments("/hubs/chat") ||
                     path.StartsWithSegments("/hubs/notifications") ||
                     path.StartsWithSegments("/supportHub")))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        };
    });

// 🌐 CORS Politikası
var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins, policy =>
    {
        policy.WithOrigins(
                "http://localhost:4200",         // Geliştirme ortamı (local)
                "https://dekofar.com",           // Production domain
                "http://212.154.77.170:4200"     // 🔒 Senin IP adresin üzerinden gelen istekler
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials(); // Gerekirse oturum bilgilerini iletmek için
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

WebApplication? app = null;

try
{
    app = builder.Build();

    // PostgreSQL bağlantısını doğrula (şifreyi loglama!)
    ValidatePostgresConnection(app.Configuration.GetConnectionString("DefaultConnection"), app.Logger);

    // Respond with 204 instead of 404 for requests like /robots933456.txt
    app.Use(async (context, next) =>
    {
        var path = context.Request.Path.Value;
        if (!string.IsNullOrEmpty(path) &&
            path.StartsWith("/robots", StringComparison.OrdinalIgnoreCase) &&
            path.EndsWith(".txt", StringComparison.OrdinalIgnoreCase))
        {
            context.Response.StatusCode = StatusCodes.Status204NoContent;
            return;
        }

        await next();
    });

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

    // 🌐 Middleware order matters for authentication and CORS
    app.UseRouting();
    app.UseCors(MyAllowSpecificOrigins);

    // Azure App Service already handles HTTPS, so avoid redirect warnings there
    if (!app.Environment.IsProduction())
    {
        app.UseHttpsRedirection();
    }

    app.UseAuthentication();
    app.UseAuthorization();
    app.UseHangfireDashboard();
    app.MapControllers();
    // Route for the SignalR chat hub used for user-to-user messaging
    app.MapHub<ChatHub>("/hubs/chat");
    app.MapHub<NotificationHub>("/hubs/notifications");
    app.MapHub<SupportHub>("/supportHub");

    // 🚀 Seed default roles and admin user
    //await SeedData.SeedDefaultsAsync(app.Services);

    var configuration = app.Services.GetRequiredService<IConfiguration>();

    // Test verilerini oluştururken hataları yakala ve logla
    try
    {
        var enableTestData = configuration.GetValue<bool>("EnableTestData");
        if (enableTestData)
        {
            await TestDataSeeder.SeedAsync(app.Services, enableTestData);
        }
    }
    catch (Exception seedingEx)
    {
        app.Logger.LogError(seedingEx, "Test verileri oluşturulurken hata meydana geldi");
    }

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
}
catch (Exception ex)
{
    var logger = app?.Logger ?? LoggerFactory.Create(c => c.AddConsole()).CreateLogger("Program");
    logger.LogCritical(ex, "Uygulama başlatılırken beklenmeyen bir hata oluştu");
}

// PostgreSQL bağlantı dizesini güvenli şekilde doğrulamak için yardımcı metot
static void ValidatePostgresConnection(string? connectionString, ILogger logger)
{
    if (string.IsNullOrWhiteSpace(connectionString))
    {
        logger.LogWarning("PostgreSQL connection string is not configured");
        return;
    }

    try
    {
        var builder = new NpgsqlConnectionStringBuilder(connectionString);

        // Hassas bilgileri loglamadan bağlanılacak sunucuyu göster
        logger.LogInformation("PostgreSQL'e bağlanma denemesi Host:{Host} DB:{Database} User:{Username}",
            builder.Host, builder.Database, builder.Username);

        using var connection = new NpgsqlConnection(builder.ConnectionString);
        connection.Open(); // Doğrulama amaçlı kısa bağlantı
    }
    catch (NpgsqlException ex)
    {
        logger.LogError(ex, "PostgreSQL bağlantı hatası. SqlState: {SqlState}", ex.SqlState);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "PostgreSQL connection string doğrulanamadı");
    }
}
