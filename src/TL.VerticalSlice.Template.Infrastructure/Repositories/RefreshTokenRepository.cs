using Dapper;
using TL.VerticalSlice.Template.Application.Contracts.Repositories;
using TL.VerticalSlice.Template.Domain.Entities;
using TL.VerticalSlice.Template.Infrastructure.Data;

namespace TL.VerticalSlice.Template.Infrastructure.Repositories;

public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public RefreshTokenRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<RefreshToken?> GetByTokenAsync(string token)
    {
        const string sql = @"
            SELECT Id, UserId, Token, ExpiresAt, CreatedAt, IsRevoked, RevokedAt
            FROM RefreshTokens
            WHERE Token = @Token";

        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<RefreshToken>(sql, new { Token = token });
    }

    public async Task<RefreshToken?> GetByIdAsync(int id)
    {
        const string sql = @"
            SELECT Id, UserId, Token, ExpiresAt, CreatedAt, IsRevoked, RevokedAt
            FROM RefreshTokens
            WHERE Id = @Id";

        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<RefreshToken>(sql, new { Id = id });
    }

    public async Task<IEnumerable<RefreshToken>> GetActiveByUserIdAsync(string userId)
    {
        const string sql = @"
            SELECT Id, UserId, Token, ExpiresAt, CreatedAt, IsRevoked, RevokedAt
            FROM RefreshTokens
            WHERE UserId = @UserId
            AND IsRevoked = 0
            AND ExpiresAt > GETUTCDATE()
            ORDER BY CreatedAt DESC";

        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<RefreshToken>(sql, new { UserId = userId });
    }

    public async Task<int> CreateAsync(RefreshToken refreshToken)
    {
        const string sql = @"
            INSERT INTO RefreshTokens (UserId, Token, ExpiresAt, CreatedAt, IsRevoked, RevokedAt)
            VALUES (@UserId, @Token, @ExpiresAt, @CreatedAt, @IsRevoked, @RevokedAt);
            SELECT CAST(SCOPE_IDENTITY() AS INT);";

        using var connection = _connectionFactory.CreateConnection();
        return await connection.QuerySingleAsync<int>(sql, refreshToken);
    }

    public async Task RevokeAsync(int id)
    {
        const string sql = @"
            UPDATE RefreshTokens
            SET IsRevoked = 1,
                RevokedAt = GETUTCDATE()
            WHERE Id = @Id";

        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(sql, new { Id = id });
    }

    public async Task RevokeAllByUserAsync(string userId)
    {
        const string sql = @"
            UPDATE RefreshTokens
            SET IsRevoked = 1,
                RevokedAt = GETUTCDATE()
            WHERE UserId = @UserId AND IsRevoked = 0";

        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(sql, new { UserId = userId });
    }

    public async Task DeleteExpiredAsync()
    {
        const string sql = "DELETE FROM RefreshTokens WHERE ExpiresAt < GETUTCDATE()";

        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(sql);
    }
}

