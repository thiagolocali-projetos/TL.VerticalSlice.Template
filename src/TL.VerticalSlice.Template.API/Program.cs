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

// ---- Serilog: configuration MUST come first ----
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
    .MinimumLevel.Override("System", Serilog.Events.LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    // OPTIONAL: Uncomment the next line to enable Seq structured logging aggregation
    //.WriteTo.Seq("http://localhost:5341")
    .CreateLogger();

try
{
    Log.Information("Starting TL.VerticalSlice.Template API...");

    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog();

    // ---- JWT Configuration ----
    var jwtSettings = builder.Configuration.GetSection("JwtSettings");
    var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured");
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

    // ---- Services ----
    builder.Services.AddControllers();
    builder.Services.AddApplicationServices(builder.Configuration);
    builder.Services.AddInfrastructureServices(builder.Configuration);
    // OPTIONAL: Uncomment to enable Hangfire background job processing
    // builder.Services.AddHangfireServices(builder.Configuration);
    builder.Services.AddSwaggerServices();
    builder.Services.AddApplicationHealthChecks(builder.Configuration);

    // OPTIONAL: Uncomment to enable OpenTelemetry distributed tracing to Jaeger
    // builder.Services.AddOpenTelemetry()
    //     .ConfigureResource(r => r.AddService("TL.VerticalSlice.Template.API"))
    //     .WithTracing(t => t
    //         .AddAspNetCoreInstrumentation()
    //         .AddOtlpExporter(o => o.Endpoint = new Uri("http://localhost:4317")));

    var app = builder.Build();

    // ---- Pipeline ----
    app.UseSerilogRequestLogging();
    app.UseRateLimiting(requestsPerMinute: 100);
    app.UseMiddleware<ExceptionHandlingMiddleware>();
    // OPTIONAL: Uncomment to enable Hangfire dashboard (requires AddHangfireServices above)
    // app.UseHangfireConfiguration();

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

    
// Health endpoint para monitoramento de deployment
app.MapGet("/", () => new
{
    status = "OK",
    environment = app.Environment.EnvironmentName,
    timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")
});
app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "API terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

/// <summary>
/// Explicit Program class to make it accessible to integration tests.
/// </summary>
public partial class Program { }

