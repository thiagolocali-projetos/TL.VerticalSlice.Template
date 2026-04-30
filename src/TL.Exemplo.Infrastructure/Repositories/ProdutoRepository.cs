using Dapper;
using Microsoft.Extensions.Logging;
using TL.Exemplo.Application.Contracts.Repositories;
using TL.Exemplo.Domain.Entities;
using TL.Exemplo.Infrastructure.Data;

namespace TL.Exemplo.Infrastructure.Repositories;

public class ProdutoRepository : IProdutoRepository
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly ILogger<ProdutoRepository> _logger;

    public ProdutoRepository(
        IDbConnectionFactory connectionFactory
        , ILogger<ProdutoRepository> logger
        )
    {
        _logger = logger;
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<Produto>> GetAllAsync()
    {
        _logger.LogInformation("Recuperando todos os produtos");

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

    public async Task<TL.Exemplo.Application.Common.Models.PagedResult<Produto>> GetPagedAsync(int pageNumber = 1, int pageSize = 20)
    {
        _logger.LogInformation("Recuperando produtos paginados: Page={PageNumber}, Size={PageSize}", pageNumber, pageSize);

        // Validar parâmetros
        pageNumber = pageNumber < 1 ? 1 : pageNumber;
        pageSize = pageSize > 100 ? 100 : pageSize < 1 ? 1 : pageSize;

        const string countSql = "SELECT COUNT(*) FROM Produtos";
        const string dataSql = @"
            SELECT Id, Nome, Descricao, Preco, QuantidadeEstoque, Ativo, CriadoEm, AtualizadoEm
            FROM Produtos
            ORDER BY Nome
            OFFSET (@PageNumber - 1) * @PageSize ROWS
            FETCH NEXT @PageSize ROWS ONLY";

        using var connection = _connectionFactory.CreateConnection();

        var totalCount = await connection.QueryFirstOrDefaultAsync<int>(countSql);
        var produtos = await connection.QueryAsync<Produto>(dataSql, new { PageNumber = pageNumber, PageSize = pageSize });

        return TL.Exemplo.Application.Common.Models.PagedResult<Produto>.Create(
            produtos,
            pageNumber,
            pageSize,
            totalCount
        );
    }

    public async Task<TL.Exemplo.Application.Common.Models.PagedResult<Produto>> GetPagedAtivosAsync(int pageNumber = 1, int pageSize = 20)
    {
        _logger.LogInformation("Recuperando produtos ativos paginados: Page={PageNumber}, Size={PageSize}", pageNumber, pageSize);

        // Validar parâmetros
        pageNumber = pageNumber < 1 ? 1 : pageNumber;
        pageSize = pageSize > 100 ? 100 : pageSize < 1 ? 1 : pageSize;

        const string countSql = "SELECT COUNT(*) FROM Produtos WHERE Ativo = 1";
        const string dataSql = @"
            SELECT Id, Nome, Descricao, Preco, QuantidadeEstoque, Ativo, CriadoEm, AtualizadoEm
            FROM Produtos
            WHERE Ativo = 1
            ORDER BY Nome
            OFFSET (@PageNumber - 1) * @PageSize ROWS
            FETCH NEXT @PageSize ROWS ONLY";

        using var connection = _connectionFactory.CreateConnection();

        var totalCount = await connection.QueryFirstOrDefaultAsync<int>(countSql);
        var produtos = await connection.QueryAsync<Produto>(dataSql, new { PageNumber = pageNumber, PageSize = pageSize });

        return TL.Exemplo.Application.Common.Models.PagedResult<Produto>.Create(
            produtos,
            pageNumber,
            pageSize,
            totalCount
        );
    }
}
