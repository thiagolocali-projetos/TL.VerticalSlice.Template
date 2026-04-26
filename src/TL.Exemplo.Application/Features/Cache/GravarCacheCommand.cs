using MediatR;
using TL.Exemplo.Application.Common.Models;
using TL.Exemplo.Application.Contracts.Cache;

namespace TL.Exemplo.Application.Features.Cache.GravarCache;

public record GravarCacheCommand(string Key, string Valor, int? MinutosExpiracao = 5) : IRequest<ApiResponse<object>>;

public class GravarCacheHandler : IRequestHandler<GravarCacheCommand, ApiResponse<object>>
{
    private readonly ICacheService _cache;

    public GravarCacheHandler(ICacheService cache) => _cache = cache;

    public async Task<ApiResponse<object>> Handle(GravarCacheCommand request, CancellationToken ct)
    {
        var expiration = TimeSpan.FromMinutes(request.MinutosExpiracao ?? 5);
        await _cache.SetAsync(request.Key, request.Valor, expiration, ct);

        return ApiResponse<object>.Ok(new { request.Key, request.Valor, Expiracao = expiration }, "Valor gravado no cache");
    }
}