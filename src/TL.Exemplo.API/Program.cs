using TL.Exemplo.API.Extensions;
using TL.Exemplo.API.Middleware;

var builder = WebApplication.CreateBuilder(args);

// ── Serviços ───────────────────────────────────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddSwaggerServices();

var app = builder.Build();

// ── Pipeline ───────────────────────────────────────────────────────────────
// Middleware global de tratamento de exceções (deve ser o primeiro)
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "TL.Exemplo API v1");
        options.RoutePrefix = string.Empty; // Swagger na raiz: https://localhost:{porta}/
        options.DocumentTitle = "TL.Exemplo - Vertical Slice API";
    });
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
