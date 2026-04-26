using Dapper;
using TL.Exemplo.Application.Contracts.Repositories;
using TL.Exemplo.Domain.Entities;
using TL.Exemplo.Infrastructure.Data;

namespace TL.Exemplo.Infrastructure.Repositories;

public class ProdutoRepository : IProdutoRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public ProdutoRepository(IDbConnectionFactory connectionFactory)
        => _connectionFactory = connectionFactory;

    public async Task<IEnumerable<Produto>> GetAllAsync()
    {
        const string sql = @"
            SELECT Id, Nome, Descricao, Preco, QuantidadeEstoque, Ativo, CriadoEm, AtualizadoEm
            FROM Produtos
            ORDER BY Nome";

        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<Produto>(sql);
    }

    public async Task<IEnumerable<Produto>> GetAllAtivosAsync()
    {
        const string sql = @"
            SELECT Id, Nome, Descricao, Preco, QuantidadeEstoque, Ativo, CriadoEm, AtualizadoEm
            FROM Produtos
            WHERE Ativo = 1
            ORDER BY Nome";

        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<Produto>(sql);
    }

    public async Task<Produto?> GetByIdAsync(int id)
    {
        const string sql = @"
            SELECT Id, Nome, Descricao, Preco, QuantidadeEstoque, Ativo, CriadoEm, AtualizadoEm
            FROM Produtos
            WHERE Id = @Id";

        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<Produto>(sql, new { Id = id });
    }

    public async Task<int> CreateAsync(Produto produto)
    {
        const string sql = @"
            INSERT INTO Produtos (Nome, Descricao, Preco, QuantidadeEstoque, Ativo, CriadoEm)
            VALUES (@Nome, @Descricao, @Preco, @QuantidadeEstoque, @Ativo, @CriadoEm);
            SELECT CAST(SCOPE_IDENTITY() AS INT);";

        using var connection = _connectionFactory.CreateConnection();
        return await connection.QuerySingleAsync<int>(sql, produto);
    }

    public async Task UpdateAsync(Produto produto)
    {
        const string sql = @"
            UPDATE Produtos
            SET Nome              = @Nome,
                Descricao         = @Descricao,
                Preco             = @Preco,
                QuantidadeEstoque = @QuantidadeEstoque,
                Ativo             = @Ativo,
                AtualizadoEm      = @AtualizadoEm
            WHERE Id = @Id";

        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(sql, produto);
    }

    public async Task DeleteAsync(int id)
    {
        const string sql = "DELETE FROM Produtos WHERE Id = @Id";

        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(sql, new { Id = id });
    }
}
