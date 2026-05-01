using Dapper;
using Microsoft.Extensions.Logging;
using TL.VerticalSlice.Template.Application.Contracts.Repositories;
using TL.VerticalSlice.Template.Domain.Entities;
using TL.VerticalSlice.Template.Infrastructure.Data;

namespace TL.VerticalSlice.Template.Infrastructure.Repositories;

public class SampleRepository : ISampleRepository
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly ILogger<SampleRepository> _logger;

    public SampleRepository(
        IDbConnectionFactory connectionFactory
        , ILogger<SampleRepository> logger
        )
    {
        _logger = logger;
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<Sample>> GetAllAsync()
    {
        _logger.LogInformation("Recuperando todos os Samples");

        const string sql = @"
            SELECT Id, Nome, Descricao, Preco, QuantidadeEstoque, Ativo, CriadoEm, AtualizadoEm
            FROM Samples
            ORDER BY Nome";

        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<Sample>(sql);
    }

    public async Task<IEnumerable<Sample>> GetAllAtivosAsync()
    {
        const string sql = @"
            SELECT Id, Nome, Descricao, Preco, QuantidadeEstoque, Ativo, CriadoEm, AtualizadoEm
            FROM Samples
            WHERE Ativo = 1
            ORDER BY Nome";

        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<Sample>(sql);
    }

    public async Task<Sample?> GetByIdAsync(int id)
    {
        const string sql = @"
            SELECT Id, Nome, Descricao, Preco, QuantidadeEstoque, Ativo, CriadoEm, AtualizadoEm
            FROM Samples
            WHERE Id = @Id";

        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<Sample>(sql, new { Id = id });
    }

    public async Task<int> CreateAsync(Sample Sample)
    {
        const string sql = @"
            INSERT INTO Samples (Nome, Descricao, Preco, QuantidadeEstoque, Ativo, CriadoEm)
            VALUES (@Nome, @Descricao, @Preco, @QuantidadeEstoque, @Ativo, @CriadoEm);
            SELECT CAST(SCOPE_IDENTITY() AS INT);";

        using var connection = _connectionFactory.CreateConnection();
        return await connection.QuerySingleAsync<int>(sql, Sample);
    }

    public async Task UpdateAsync(Sample Sample)
    {
        const string sql = @"
            UPDATE Samples
            SET Nome              = @Nome,
                Descricao         = @Descricao,
                Preco             = @Preco,
                QuantidadeEstoque = @QuantidadeEstoque,
                Ativo             = @Ativo,
                AtualizadoEm      = @AtualizadoEm
            WHERE Id = @Id";

        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(sql, Sample);
    }

    public async Task DeleteAsync(int id)
    {
        const string sql = "DELETE FROM Samples WHERE Id = @Id";

        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(sql, new { Id = id });
    }

    public async Task<TL.VerticalSlice.Template.Application.Common.Models.PagedResult<Sample>> GetPagedAsync(int pageNumber = 1, int pageSize = 20)
    {
        _logger.LogInformation("Recuperando Samples paginados: Page={PageNumber}, Size={PageSize}", pageNumber, pageSize);

        // Validar parÃ¢metros
        pageNumber = pageNumber < 1 ? 1 : pageNumber;
        pageSize = pageSize > 100 ? 100 : pageSize < 1 ? 1 : pageSize;

        const string countSql = "SELECT COUNT(*) FROM Samples";
        const string dataSql = @"
            SELECT Id, Nome, Descricao, Preco, QuantidadeEstoque, Ativo, CriadoEm, AtualizadoEm
            FROM Samples
            ORDER BY Nome
            OFFSET (@PageNumber - 1) * @PageSize ROWS
            FETCH NEXT @PageSize ROWS ONLY";

        using var connection = _connectionFactory.CreateConnection();

        var totalCount = await connection.QueryFirstOrDefaultAsync<int>(countSql);
        var Samples = await connection.QueryAsync<Sample>(dataSql, new { PageNumber = pageNumber, PageSize = pageSize });

        return TL.VerticalSlice.Template.Application.Common.Models.PagedResult<Sample>.Create(
            Samples,
            pageNumber,
            pageSize,
            totalCount
        );
    }

    public async Task<TL.VerticalSlice.Template.Application.Common.Models.PagedResult<Sample>> GetPagedAtivosAsync(int pageNumber = 1, int pageSize = 20)
    {
        _logger.LogInformation("Recuperando Samples ativos paginados: Page={PageNumber}, Size={PageSize}", pageNumber, pageSize);

        // Validar parÃ¢metros
        pageNumber = pageNumber < 1 ? 1 : pageNumber;
        pageSize = pageSize > 100 ? 100 : pageSize < 1 ? 1 : pageSize;

        const string countSql = "SELECT COUNT(*) FROM Samples WHERE Ativo = 1";
        const string dataSql = @"
            SELECT Id, Nome, Descricao, Preco, QuantidadeEstoque, Ativo, CriadoEm, AtualizadoEm
            FROM Samples
            WHERE Ativo = 1
            ORDER BY Nome
            OFFSET (@PageNumber - 1) * @PageSize ROWS
            FETCH NEXT @PageSize ROWS ONLY";

        using var connection = _connectionFactory.CreateConnection();

        var totalCount = await connection.QueryFirstOrDefaultAsync<int>(countSql);
        var Samples = await connection.QueryAsync<Sample>(dataSql, new { PageNumber = pageNumber, PageSize = pageSize });

        return TL.VerticalSlice.Template.Application.Common.Models.PagedResult<Sample>.Create(
            Samples,
            pageNumber,
            pageSize,
            totalCount
        );
    }
}

