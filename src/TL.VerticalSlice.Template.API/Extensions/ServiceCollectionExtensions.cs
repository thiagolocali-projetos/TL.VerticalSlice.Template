using FluentValidation;
using MediatR;
using Microsoft.OpenApi.Models;
using TL.VerticalSlice.Template.Application.Common.Behaviors;
using TL.VerticalSlice.Template.Application.Contracts.Cache;
using TL.VerticalSlice.Template.Application.Contracts.Messaging;
using TL.VerticalSlice.Template.Application.Contracts.Repositories;
using TL.VerticalSlice.Template.Infrastructure.Cache;
using TL.VerticalSlice.Template.Infrastructure.Data;
using TL.VerticalSlice.Template.Infrastructure.Messaging;
using TL.VerticalSlice.Template.Infrastructure.Repositories;

namespace TL.VerticalSlice.Template.API.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        // MediatR â€” registra todos os handlers da Application
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(Application.Common.Models.ApiResponse<>).Assembly));

        // FluentValidation â€” registra todos os validators da Application
        services.AddValidatorsFromAssembly(typeof(Application.Common.Models.ApiResponse<>).Assembly);

        // Pipeline behavior de validaÃ§Ã£o (executa validators automaticamente antes de cada handler)
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        return services;
    }

    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("SqlServer")
            ?? throw new InvalidOperationException("Connection string 'SqlServer' nÃ£o encontrada.");

        services.AddSingleton<IDbConnectionFactory>(_ => new SqlServerConnectionFactory(connectionString));
        services.AddSingleton<IRabbitMqService, RabbitMqService>();
        services.AddSingleton<IKafkaService, KafkaService>();
        services.AddScoped<ISampleRepository, SampleRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

        // Redis Cache
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = configuration.GetConnectionString("Redis") ?? "localhost:6379";
            options.InstanceName = "TLVerticalSliceTemplate_";
        });
        services.AddSingleton<ICacheService, RedisCacheService>();

        return services;
    }

    public static IServiceCollection AddSwaggerServices(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "TL.VerticalSlice.Template - Vertical Slice API",
                Version = "v1",
                Description = "API de exemplo com arquitetura Vertical Slice, MediatR, FluentValidation e Dapper.",
                Contact = new OpenApiContact
                {
                    Name = "TL.VerticalSlice.Template",
                    Email = "contato@TLVerticalSliceTemplate.com.br"
                }
            });

            // JWT Security Definition
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                Description = "JWT Authorization header using the Bearer scheme."
            });

            // JWT Security Requirement
            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });

            // Inclui comentÃ¡rios XML do projeto (opcional, mas Ãºtil)
            var xmlFiles = Directory.GetFiles(AppContext.BaseDirectory, "*.xml");
            foreach (var xmlFile in xmlFiles)
                options.IncludeXmlComments(xmlFile);
        });

        return services;
    }
}

