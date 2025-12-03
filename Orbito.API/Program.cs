using Microsoft.OpenApi.Models;
using Orbito.API.Middleware;
using Orbito.API.HealthChecks;
using Orbito.Infrastructure.PaymentGateways.Stripe.Models;
using Orbito.Application;
using Orbito.Infrastructure;
using Serilog;
using Serilog.Events;
using System.Security.Claims;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.IO;

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/info-.txt", 
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30,
        restrictedToMinimumLevel: LogEventLevel.Information)
    .WriteTo.File("logs/errors-.txt", 
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30,
        restrictedToMinimumLevel: LogEventLevel.Error)
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

// Use Serilog
builder.Host.UseSerilog();

// Add Global Exception Handler
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddControllers();
builder.Services.AddOpenApi();

// Configure CORS from configuration (appsettings.json or appsettings.{Environment}.json)
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() 
    ?? new[] { "http://localhost:3000", "https://localhost:3000" }; // Fallback for development

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(allowedOrigins)
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials(); // Required for NextAuth cookies
    });
});

// Configure Health Checks with custom checks
builder.Services.AddHealthChecks()
    .AddCheck<StripeHealthCheck>("stripe", tags: new[] { "external" })
    .AddCheck<PaymentSystemHealthCheck>("payment_system", tags: new[] { "critical" })
    .AddDbContextCheck<Orbito.Infrastructure.Data.ApplicationDbContext>("database");

builder.Services.AddHealthChecksUI()
    .AddInMemoryStorage();

// Add Memory Cache
builder.Services.AddMemoryCache();

// Add Response Compression
builder.Services.AddResponseCompression();

// NOTE: Rate Limiting and CORS are now configured in Infrastructure layer (AddInfrastructure)
// Specific rate limit policies for webhook and API endpoints
builder.Services.AddRateLimiter(rateLimiterOptions =>
{
    // Webhook rate limiter - 100 requests per minute
    rateLimiterOptions.AddPolicy("webhook", context =>
        System.Threading.RateLimiting.RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.Request.Headers.Host.ToString(),
            factory: _ => new System.Threading.RateLimiting.FixedWindowRateLimiterOptions
            {
                PermitLimit = 100,
                Window = TimeSpan.FromMinutes(1),
                QueueProcessingOrder = System.Threading.RateLimiting.QueueProcessingOrder.OldestFirst,
                QueueLimit = 0 // No queue, reject immediately
            }));

    // Reconciliation rate limiter - 5 requests per 15 minutes per tenant
    // SECURITY: Prevents abuse of Stripe API and expensive reconciliation operations
    rateLimiterOptions.AddPolicy("reconciliation", context =>
        System.Threading.RateLimiting.RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.User.FindFirst("tenant_id")?.Value ?? context.Request.Headers.Host.ToString(),
            factory: _ => new System.Threading.RateLimiting.FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(15),
                QueueProcessingOrder = System.Threading.RateLimiting.QueueProcessingOrder.OldestFirst,
                QueueLimit = 2
            }));

    // API rate limiter - 1000 requests per minute
    rateLimiterOptions.AddPolicy("api", context =>
        System.Threading.RateLimiting.RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.Request.Headers.Host.ToString(),
            factory: _ => new System.Threading.RateLimiting.FixedWindowRateLimiterOptions
            {
                PermitLimit = 1000,
                Window = TimeSpan.FromMinutes(1),
                QueueProcessingOrder = System.Threading.RateLimiting.QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            }));
});

// Add Swagger with JWT support
builder.Services.AddSwaggerGen(option =>
{
    option.SwaggerDoc("v1", new OpenApiInfo { Title = "Orbito API", Version = "v1" });
    
    // Include XML comments for better documentation
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        option.IncludeXmlComments(xmlPath);
    }
    
    // Include XML comments from Application layer (for Commands/Queries)
    var applicationXmlFile = "Orbito.Application.xml";
    var applicationXmlPath = Path.Combine(AppContext.BaseDirectory, applicationXmlFile);
    if (File.Exists(applicationXmlPath))
    {
        option.IncludeXmlComments(applicationXmlPath);
    }
    
    option.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter a valid token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "Bearer"
    });
    option.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type=ReferenceType.SecurityScheme,
                    Id="Bearer"
                }
            },
            new string[]{}
        }
    });
});

builder.Services.AddApplication(builder.Configuration);
builder.Services.AddInfrastructure(builder.Configuration);

// Configure Stripe Webhook Settings
builder.Services.Configure<StripeWebhookSettings>(builder.Configuration.GetSection("StripeWebhookSettings"));

// Configure Monitoring Settings for Health Checks
builder.Services.Configure<MonitoringSettings>(builder.Configuration.GetSection("MonitoringSettings"));

// Add Background Jobs
builder.Services.AddHostedService<Orbito.Application.BackgroundJobs.CheckExpiringSubscriptionsJob>();
builder.Services.AddHostedService<Orbito.Application.BackgroundJobs.ProcessRecurringPaymentsJob>();
builder.Services.AddHostedService<Orbito.Application.BackgroundJobs.UpcomingPaymentReminderJob>();
builder.Services.AddHostedService<Orbito.Application.BackgroundJobs.ExpiredCardNotificationJob>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Orbito_test API");
        c.RoutePrefix = "swagger";
    });
}

app.UseHttpsRedirection();

// Use Response Compression (from Infrastructure layer)
app.UseResponseCompression();

// Use CORS (configured in Infrastructure layer + local policy)
app.UseCors();

// Add global exception handler
app.UseExceptionHandler();

// Add rate limiting
app.UseRateLimiter();

// Add Stripe signature verification middleware
app.UseStripeSignatureVerification();

// Add idempotency middleware
app.UseIdempotency();

// Add authentication and authorization
app.UseAuthentication();
app.UseAuthorization();

// Add tenant middleware (after authentication to access JWT claims)
app.UseMiddleware<TenantMiddleware>();

app.MapControllers();

app.MapGet("/", () => Results.Ok(new
{
    message = "Witaj w Orbito API!",
    endpoints = new[] {
            "/swagger - Dokumentacja API",
            "/health - Health Checks",
            "/healthchecks-ui - Health Checks UI"
        },
    timestamp = DateTime.UtcNow
}));

// Configure Health Check endpoints
app.MapHealthChecks("/health");

app.MapHealthChecksUI();

try
{
    Log.Information("Starting Orbito API");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
