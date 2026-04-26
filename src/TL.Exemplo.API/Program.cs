using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using TL.Exemplo.API.Extensions;
using TL.Exemplo.API.Middleware;

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

    // ── Serviços ───────────────────────────────────────────────
    builder.Services.AddControllers();
    builder.Services.AddApplicationServices(builder.Configuration);
    builder.Services.AddInfrastructureServices(builder.Configuration);
    builder.Services.AddSwaggerServices();
    builder.Services.AddOpenTelemetry()
    .ConfigureResource(r => r.AddService("TL.Exemplo.API"))
    .WithTracing(t => t
        .AddAspNetCoreInstrumentation()
        .AddOtlpExporter(o => o.Endpoint = new Uri("http://localhost:4317")));

    var app = builder.Build();

    // ── Pipeline ───────────────────────────────────────────────
    app.UseSerilogRequestLogging(); // Log automático de cada request
    app.UseMiddleware<ExceptionHandlingMiddleware>();

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
    app.UseAuthorization();
    app.MapControllers();

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