using Microsoft.OpenApi.Models;
using Orbito.API.Middleware;
using Orbito.Infrastructure.PaymentGateways.Stripe.Models;
using Orbito.Application;
using Orbito.Infrastructure;
using Serilog;
using Serilog.Events;
using System.Security.Claims;
using Microsoft.AspNetCore.Diagnostics;

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
builder.Services.AddHealthChecks();
builder.Services.AddHealthChecksUI()
    .AddInMemoryStorage();

// Add Rate Limiting
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

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("Orbito_test", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "http://127.0.0.1:3000")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Add Swagger with JWT support
builder.Services.AddSwaggerGen(option =>
{
    option.SwaggerDoc("v1", new OpenApiInfo { Title = "Demo API", Version = "v1" });
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

// Use CORS
app.UseCors("Orbito_test");

// Add global exception handler
app.UseExceptionHandler();

// Add rate limiting
app.UseRateLimiter();

// Add tenant middleware
app.UseMiddleware<TenantMiddleware>();

// Add Stripe signature verification middleware
app.UseStripeSignatureVerification();

// Add authentication and authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapGet("/", () => Results.Ok(new
{
    message = "Witaj w Orbito API!",
    endpoints = new[] {
            "/swagger - Dokumentacja API",
        },
    timestamp = DateTime.UtcNow
}));
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
