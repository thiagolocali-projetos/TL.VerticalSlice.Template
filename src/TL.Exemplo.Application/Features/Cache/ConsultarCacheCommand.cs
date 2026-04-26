using MediatR;
using TL.Exemplo.Application.Common.Models;
using TL.Exemplo.Application.Contracts.Cache;

namespace TL.Exemplo.Application.Features.Cache.ConsultarCache;

public record ConsultarCacheCommand(string Key) : IRequest<ApiResponse<object>>;

public class ConsultarCacheHandler : IRequestHandler<ConsultarCacheCommand, ApiResponse<object>>
{
    private readonly ICacheService _cache;

    public ConsultarCacheHandler(ICacheService cache) => _cache = cache;

    public async Task<ApiResponse<object>> Handle(ConsultarCacheCommand request, CancellationToken ct)
    {
        var valor = await _cache.GetAsync<string>(request.Key, ct);

        if (valor is null)
            return ApiResponse<object>.NaoEncontrado($"Key '{request.Key}' não encontrada no cache");

        return ApiResponse<object>.Ok(new { request.Key, Valor = valor }, "Valor encontrado no cache");
    }
}