using HotelPro.Infrastructure.Data;
using HotelPro.Infrastructure.Data.Interceptors;
using HotelPro.Infrastructure.BackgroundJobs;
using HotelPro.Infrastructure.Services;
using HotelPro.Core.Services;
using HotelPro.Api.Extensions;
using HotelPro.Api.Middleware;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using System.Threading.RateLimiting;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// API Versioning
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(2, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = ApiVersionReader.Combine(
        new HeaderApiVersionReader("api-version"),
        new QueryStringApiVersionReader("api-version"));
})
.AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

// Swagger
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "HotelPRO API", Version = "v1" });
    c.SwaggerDoc("v2", new OpenApiInfo { Title = "HotelPRO API", Version = "v2" });
});

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Rate Limiting
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.AddPolicy("Staff", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: "staff",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 100,
                Window = TimeSpan.FromSeconds(10),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 10
            }));

    options.AddPolicy("Guest", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: "guest",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromSeconds(10),
                QueueLimit = 5
            }));

    options.AddPolicy("Auth", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: "auth",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            }));

    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
    {
        var path = context.Request.Path.Value?.ToLowerInvariant() ?? "";
        if (path.StartsWith("/api/auth") || path.StartsWith("/api/login"))
            return RateLimitPartition.GetFixedWindowLimiter("Auth", _ => new FixedWindowRateLimiterOptions { PermitLimit = 5, Window = TimeSpan.FromMinutes(1) });
        if (path.StartsWith("/api/guest") || path.StartsWith("/api/public"))
            return RateLimitPartition.GetFixedWindowLimiter("Guest", _ => new FixedWindowRateLimiterOptions { PermitLimit = 10, Window = TimeSpan.FromSeconds(10) });
        return RateLimitPartition.GetFixedWindowLimiter("Staff", _ => new FixedWindowRateLimiterOptions { PermitLimit = 100, Window = TimeSpan.FromSeconds(10) });
    });
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<AuditInterceptor>();

builder.Services.AddMemoryCache();
builder.Services.AddScoped<IFeatureFlagService, FeatureFlagService>();
builder.Services.AddScoped<HotelPro.Core.Services.IConfigurationService, HotelPro.Infrastructure.Services.ConfigurationService>();
builder.Services.AddSingleton<ITranslationService, TranslationService>();
builder.Services.AddScoped<ITenantService, TenantService>();
builder.Services.AddScoped<HotelPro.Core.Services.IRoomService, HotelPro.Infrastructure.Services.RoomService>();
builder.Services.AddScoped<HotelPro.Core.Services.IRoomOccupancyPolicy, HotelPro.Infrastructure.Services.RoomOccupancyPolicy>();
builder.Services.AddScoped<HotelPro.Core.Services.IRoomStatusBroadcaster, HotelPro.Api.Hubs.SignalRBroadcaster>();
builder.Services.AddScoped<HotelPro.Core.Interfaces.IBookingRepository, HotelPro.Infrastructure.Repositories.BookingRepository>();
builder.Services.AddScoped<HotelPro.Core.Services.IBookingService, HotelPro.Infrastructure.Services.BookingService>();
builder.Services.AddScoped<HotelPro.Core.Services.IBookingAvailabilityService, HotelPro.Infrastructure.Services.BookingAvailabilityService>();
builder.Services.Configure<HotelPro.Core.Services.EmailConfiguration>(builder.Configuration.GetSection(HotelPro.Core.Services.EmailConfiguration.SectionName));
builder.Services.AddTransient<HotelPro.Infrastructure.Services.ISmtpClient, HotelPro.Infrastructure.Services.MailKitSmtpClient>();
builder.Services.AddScoped<HotelPro.Core.Services.IEmailService, HotelPro.Infrastructure.Services.EmailService>();
builder.Services.AddScoped<HotelPro.Core.Interfaces.IBookingGroupRepository, HotelPro.Infrastructure.Repositories.BookingGroupRepository>();
builder.Services.AddScoped<HotelPro.Core.Services.IBookingGroupService, HotelPro.Infrastructure.Services.BookingGroupService>();
builder.Services.AddScoped<HotelPro.Core.Services.IFolioService, HotelPro.Infrastructure.Services.FolioService>();
builder.Services.AddScoped<HotelPro.Core.Services.ICheckInService, HotelPro.Infrastructure.Services.CheckInService>();
builder.Services.AddScoped<HotelPro.Core.Services.IStayLifecycleService, HotelPro.Infrastructure.Services.StayLifecycleService>();
builder.Services.AddScoped<HotelPro.Core.Services.INightLedgerService, HotelPro.Infrastructure.Services.NightLedgerService>();
builder.Services.AddScoped<HotelPro.Core.Services.ICheckOutWorkflowService, HotelPro.Infrastructure.Services.CheckOutWorkflowService>();
builder.Services.AddScoped<HotelPro.Core.Services.IFolioLedgerService, HotelPro.Infrastructure.Services.FolioLedgerService>();
builder.Services.AddScoped<HotelPro.Core.Services.IInvoiceWorkflowService, HotelPro.Infrastructure.Services.InvoiceWorkflowService>();
builder.Services.AddScoped<HotelPro.Core.Services.IPaymentAllocationService, HotelPro.Infrastructure.Services.PaymentAllocationService>();
builder.Services.AddScoped<HotelPro.Core.Services.IReservationPolicyService, HotelPro.Infrastructure.Services.ReservationPolicyService>();
builder.Services.AddScoped<HotelPro.Core.Services.ICheckOutService, HotelPro.Infrastructure.Services.CheckOutService>();
builder.Services.AddScoped<HotelPro.Core.Services.INightAuditService, HotelPro.Infrastructure.Services.NightAuditService>();
builder.Services.AddScoped<HotelPro.Core.Services.IGuestService, HotelPro.Infrastructure.Services.GuestService>();
builder.Services.AddScoped<HotelPro.Core.Services.IInvoiceGenerator, HotelPro.Infrastructure.Services.InvoiceGenerator>();
builder.Services.AddScoped<HotelPro.Core.Services.IProformaService, HotelPro.Infrastructure.Services.ProformaService>();
builder.Services.AddScoped<HotelPro.Core.Services.IAdvancePaymentService, HotelPro.Infrastructure.Services.AdvancePaymentService>();
builder.Services.AddScoped<HotelPro.Core.Services.IExchangeRateService, HotelPro.Infrastructure.Services.ExchangeRateService>();
builder.Services.AddHttpClient("ExchangeRates", c => c.Timeout = TimeSpan.FromSeconds(10));
builder.Services.AddScoped<HotelPro.Infrastructure.Services.ReportsService>();
builder.Services.AddSingleton<HotelPro.Infrastructure.Services.BridgeService>();

builder.Services.AddSignalR();

// JWT Authentication
var jwtSecret = builder.Configuration["HOTEL_JWT_SECRET"]
    ?? Environment.GetEnvironmentVariable("HOTEL_JWT_SECRET")
    ?? throw new InvalidOperationException("HOTEL_JWT_SECRET is not configured");
var jwtIssuer = builder.Configuration["HOTEL_JWT_ISSUER"] ?? "hotelpro.local";
var jwtAudience = builder.Configuration["HOTEL_JWT_AUDIENCE"] ?? "hotelpro-api";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("Management", policy => policy.RequireRole("Admin", "Manager"));
    options.AddPolicy("CanManageRooms", policy =>
        policy.RequireAssertion(context =>
            context.User.IsInRole("Admin") || context.User.IsInRole("Manager") || context.User.IsInRole("Reception")));
    options.AddPolicy("CanManageBookings", policy =>
        policy.RequireAssertion(context =>
            context.User.IsInRole("Admin") || context.User.IsInRole("Manager") || context.User.IsInRole("Reception")));
    options.AddPolicy("CanViewReports", policy =>
        policy.RequireAssertion(context =>
            context.User.IsInRole("Admin") || context.User.IsInRole("Manager")));
    options.AddPolicy("CanHousekeep", policy =>
        policy.RequireAssertion(context =>
            context.User.IsInRole("Admin") || context.User.IsInRole("Housekeeping")));
});

// JWT Service
builder.Services.AddScoped<IJwtService, JwtService>();

// Database
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? Environment.GetEnvironmentVariable("HOTEL_DB_CONN");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException("HOTEL_DB_CONN environment variable is not set");
}

builder.Services.AddDbContext<HotelProDbContext>((sp, options) =>
{
    var auditInterceptor = sp.GetRequiredService<AuditInterceptor>();
    options.UseNpgsql(connectionString)
           .AddInterceptors(auditInterceptor);
});

// Scheduled Jobs
builder.Services.AddHostedService<NoShowDetectionJob>();
builder.Services.AddHostedService<NightAuditJob>();
builder.Services.AddHostedService<DailyReportJob>();
builder.Services.AddHostedService<BackupTriggerJob>();
builder.Services.AddHostedService<IoTDeviceCheckJob>();
builder.Services.AddHostedService<DndExpiryJob>();
builder.Services.AddHostedService<SessionCleanupJob>();
builder.Services.AddHostedService<HotelPro.Infrastructure.BackgroundJobs.GroupReleaseJob>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();
        foreach (var description in provider.ApiVersionDescriptions)
        {
            options.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json", description.GroupName.ToUpperInvariant());
        }
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowFrontend");
app.UseRateLimiter();
app.UseAuthentication();
app.UseTenantResolution();
app.UseAuthorization();
app.MapControllers();

app.MapHub<HotelPro.Api.Hubs.RoomStatusHub>("/hubs/room-status");

app.InitializeDatabase();

app.MapGet("/api/health", async ([FromServices] HotelProDbContext dbContext) =>
{
    try
    {
        await dbContext.Database.CanConnectAsync();
        return Results.Ok(new { status = "healthy", database = "healthy", timestamp = DateTime.UtcNow });
    }
    catch
    {
        return Results.Problem("Database connection failed", statusCode: 503);
    }
})
.RequireRateLimiting("Staff")
.WithName("HealthCheck")
.WithOpenApi();

app.Run();

public partial class Program { }
