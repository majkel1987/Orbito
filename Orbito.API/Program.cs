using Microsoft.OpenApi.Models;
using Orbito.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

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
app.UseCors("AllowReactApp");

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

app.Run();
