using TL.Exemplo.Domain.Entities;

namespace TL.Exemplo.Application.Contracts.Repositories;

public interface IRefreshTokenRepository
{
    Task<RefreshToken?> GetByTokenAsync(string token);
    Task<RefreshToken?> GetByIdAsync(int id);
    Task<IEnumerable<RefreshToken>> GetActiveByUserIdAsync(string userId);
    Task<int> CreateAsync(RefreshToken refreshToken);
    Task RevokeAsync(int id);
    Task RevokeAllByUserAsync(string userId);
    Task DeleteExpiredAsync();
}
