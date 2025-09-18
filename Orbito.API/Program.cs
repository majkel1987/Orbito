using Microsoft.OpenApi.Models;
using Orbito.API.Middleware;
using Orbito.Application;
using Orbito.Infrastructure;
using Serilog;
using Serilog.Events;
using System.Security.Claims;

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

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddHealthChecks();
builder.Services.AddHealthChecksUI()
    .AddInMemoryStorage();

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

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication(builder.Configuration);

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

// Add tenant middleware
app.UseMiddleware<TenantMiddleware>();

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
