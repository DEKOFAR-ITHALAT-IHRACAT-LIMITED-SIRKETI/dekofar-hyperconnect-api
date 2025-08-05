var builder = WebApplication.CreateBuilder(args);

// 🌐 CORS Politikası
// Hem local geliştirme (localhost:4200) hem production (dekofar.com) için CORS ayarı yapılır
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


// 📦 Altyapı servisleri (DbContext, Identity, JWT vs.)
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddMemoryCache();
builder.Services.AddApplication();

// Yetkilendirme politikaları
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

// Hangfire - Arkaplan görevleri
builder.Services.AddHangfire(config =>
{
    config.UseMemoryStorage();
});
builder.Services.AddHangfireServer();

// Entegrasyon servisleri
builder.Services.AddScoped<INetGsmSmsService, NetGsmSmsService>();
builder.Services.AddHttpClient<IShopifyService, ShopifyService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IModerationService, ModerationService>();

// Controller ayarları ve JSON yapılandırması
builder.Services.AddControllers()
    .AddNewtonsoftJson(options =>
    {
        options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
        options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
    });

builder.Services.AddSignalR();

// Swagger dokümantasyonu
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

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);

    c.UseAllOfToExtendReferenceSchemas();
    c.DescribeAllParametersInCamelCase();
});

// Logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

var app = builder.Build();

// 🧪 Swagger arayüzü (her ortamda açık olabilir)
if (app.Environment.IsDevelopment() || app.Environment.IsStaging() || app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Dekofar API v1");
        c.RoutePrefix = "swagger";
    });
}

// 🌐 Middleware sıralaması çok önemlidir
app.UseHttpsRedirection();

app.UseRouting(); // Rotaları başlat

app.UseCors(MyAllowSpecificOrigins); // ✅ CORS middleware'i en üstte çağrılır

app.UseAuthentication();
app.UseAuthorization();

app.UseHangfireDashboard();

app.MapControllers();

// SignalR hub endpoint'leri
app.MapHub<ChatHub>("/hubs/chat");
app.MapHub<NotificationHub>("/hubs/notifications");
app.MapHub<SupportHub>("/supportHub");

// Opsiyonel: Seed işlemleri
// await SeedData.SeedDefaultsAsync(app.Services);

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
