using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using System.Text;
using TL.Exemplo.API.Extensions;
using TL.Exemplo.API.Middleware;
using TL.Exemplo.Application.Contracts.Authentication;
using TL.Exemplo.Infrastructure.Authentication;
using Hangfire;
using Hangfire.Dashboard;

// ── Serilog: configuração DEVE vir antes de tudo ────────────────
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
    .MinimumLevel.Override("System", Serilog.Events.LogEventLevel.Warning)
    .Enrich.FromLogContext()
    //.Enrich.WithMachineName()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.Seq("http://localhost:5341")
    .CreateLogger();

try
{
    Log.Information("🚀 Iniciando TL.Exemplo API...");

    var builder = WebApplication.CreateBuilder(args);

    // Remove loggers padrão e usa Serilog
    builder.Host.UseSerilog();

    // ── JWT Configuration ──────────────────────────────────────────
    var jwtSettings = builder.Configuration.GetSection("JwtSettings");
    var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey não configurada");
    var issuer = jwtSettings["Issuer"] ?? "TL.Exemplo.API";
    var audience = jwtSettings["Audience"] ?? "TL.Exemplo.Users";

    builder.Services.AddSingleton<ITokenService>(
        new JwtTokenService(secretKey, issuer, audience, expirationMinutes: 60)
    );
    builder.Services.AddSingleton<IUserRepository, UserRepository>();

    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
            ValidateIssuer = true,
            ValidIssuer = issuer,
            ValidateAudience = true,
            ValidAudience = audience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

    // ── Serviços ───────────────────────────────────────────────
    builder.Services.AddControllers();
    builder.Services.AddApplicationServices(builder.Configuration);
    builder.Services.AddInfrastructureServices(builder.Configuration);
    builder.Services.AddHangfireServices(builder.Configuration);
    builder.Services.AddSwaggerServices();
    builder.Services.AddApplicationHealthChecks(builder.Configuration);
    builder.Services.AddOpenTelemetry()
    .ConfigureResource(r => r.AddService("TL.Exemplo.API"))
    .WithTracing(t => t
        .AddAspNetCoreInstrumentation()
        .AddOtlpExporter(o => o.Endpoint = new Uri("http://localhost:4317")));

    var app = builder.Build();

    // ── Pipeline ───────────────────────────────────────────────
    app.UseSerilogRequestLogging(); // Log automático de cada request
    app.UseRateLimiting(requestsPerMinute: 100); // Rate limiting: 100 requisições por minuto por IP
    app.UseMiddleware<ExceptionHandlingMiddleware>();
    app.UseHangfireConfiguration(); // Hangfire Dashboard + Recurring Jobs

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "TL.Exemplo API v1");
            options.RoutePrefix = string.Empty;
            options.DocumentTitle = "TL.Exemplo - Vertical Slice API";
        });
    }

    app.UseHttpsRedirection();
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();
    app.MapApplicationHealthChecks();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "❌ API terminou inesperadamente");
}
finally
{
    Log.CloseAndFlush();
}

/// <summary>
/// Classe Program explícita para tornar acessível aos testes de integração.
/// </summary>
public partial class Program { }