using FluentValidation;
using MediatR;
using TL.Exemplo.Application.Common.Behaviors;
using TL.Exemplo.Application.Contracts.Repositories;
using TL.Exemplo.Infrastructure.Data;
using TL.Exemplo.Infrastructure.Repositories;

namespace TL.Exemplo.API.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        // MediatR — registra todos os handlers da Application
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(Application.Common.Models.ApiResponse<>).Assembly));

        // FluentValidation — registra todos os validators da Application
        services.AddValidatorsFromAssembly(typeof(Application.Common.Models.ApiResponse<>).Assembly);

        // Pipeline behavior de validação (executa validators automaticamente antes de cada handler)
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        return services;
    }

    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("SqlServer")
            ?? throw new InvalidOperationException("Connection string 'SqlServer' não encontrada.");

        services.AddSingleton<IDbConnectionFactory>(_ => new SqlServerConnectionFactory(connectionString));
        services.AddScoped<IProdutoRepository, ProdutoRepository>();

        return services;
    }

    public static IServiceCollection AddSwaggerServices(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
            {
                Title = "TL.Exemplo - Vertical Slice API",
                Version = "v1",
                Description = "API de exemplo com arquitetura Vertical Slice, MediatR, FluentValidation e Dapper.",
                Contact = new Microsoft.OpenApi.Models.OpenApiContact
                {
                    Name = "TL.Exemplo",
                    Email = "contato@tlexemplo.com.br"
                }
            });

            // Inclui comentários XML do projeto (opcional, mas útil)
            var xmlFiles = Directory.GetFiles(AppContext.BaseDirectory, "*.xml");
            foreach (var xmlFile in xmlFiles)
                options.IncludeXmlComments(xmlFile);
        });

        return services;
    }
}
