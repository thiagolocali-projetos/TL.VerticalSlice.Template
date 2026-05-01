using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using System.Text;
using TL.VerticalSlice.Template.API.Extensions;
using TL.VerticalSlice.Template.API.Middleware;
using TL.VerticalSlice.Template.Application.Contracts.Authentication;
using TL.VerticalSlice.Template.Infrastructure.Authentication;
using Hangfire;
using Hangfire.Dashboard;

// â”€â”€ Serilog: configuraÃ§Ã£o DEVE vir antes de tudo â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
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
    Log.Information("ðŸš€ Iniciando TL.VerticalSlice.Template API...");

    var builder = WebApplication.CreateBuilder(args);

    // Remove loggers padrÃ£o e usa Serilog
    builder.Host.UseSerilog();

    // â”€â”€ JWT Configuration â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    var jwtSettings = builder.Configuration.GetSection("JwtSettings");
    var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey nÃ£o configurada");
    var issuer = jwtSettings["Issuer"] ?? "TL.VerticalSlice.Template.API";
    var audience = jwtSettings["Audience"] ?? "TL.VerticalSlice.Template.Users";

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

    // â”€â”€ ServiÃ§os â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    builder.Services.AddControllers();
    builder.Services.AddApplicationServices(builder.Configuration);
    builder.Services.AddInfrastructureServices(builder.Configuration);
    builder.Services.AddHangfireServices(builder.Configuration);
    builder.Services.AddSwaggerServices();
    builder.Services.AddApplicationHealthChecks(builder.Configuration);
    builder.Services.AddOpenTelemetry()
    .ConfigureResource(r => r.AddService("TL.VerticalSlice.Template.API"))
    .WithTracing(t => t
        .AddAspNetCoreInstrumentation()
        .AddOtlpExporter(o => o.Endpoint = new Uri("http://localhost:4317")));

    var app = builder.Build();

    // â”€â”€ Pipeline â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    app.UseSerilogRequestLogging(); // Log automÃ¡tico de cada request
    app.UseRateLimiting(requestsPerMinute: 100); // Rate limiting: 100 requisiÃ§Ãµes por minuto por IP
    app.UseMiddleware<ExceptionHandlingMiddleware>();
    app.UseHangfireConfiguration(); // Hangfire Dashboard + Recurring Jobs

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "TL.VerticalSlice.Template API v1");
            options.RoutePrefix = string.Empty;
            options.DocumentTitle = "TL.VerticalSlice.Template - Vertical Slice API";
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
    Log.Fatal(ex, "âŒ API terminou inesperadamente");
}
finally
{
    Log.CloseAndFlush();
}

/// <summary>
/// Classe Program explÃ­cita para tornar acessÃ­vel aos testes de integraÃ§Ã£o.
/// </summary>
public partial class Program { }
