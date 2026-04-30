using MediatR;
using TL.Exemplo.Application.Common.Models;
using TL.Exemplo.Application.Contracts.Repositories;

namespace TL.Exemplo.Application.Features.Produtos.Queries.GetProdutosPaged;

// ── Query ──────────────────────────────────────────────────────────────
public record GetProdutosPagedQuery(
    int PageNumber = 1,
    int PageSize = 20,
    bool? ApenasAtivos = null
) : IRequest<PagedResult<ProdutoDto>>;

// ── Handler ────────────────────────────────────────────────────────────
public class GetProdutosPagedQueryHandler : IRequestHandler<GetProdutosPagedQuery, PagedResult<ProdutoDto>>
{
    private readonly IProdutoRepository _repository;

    public GetProdutosPagedQueryHandler(IProdutoRepository repository)
        => _repository = repository;

    public async Task<PagedResult<ProdutoDto>> Handle(
        GetProdutosPagedQuery request,
        CancellationToken cancellationToken)
    {
        var pagedResult = request.ApenasAtivos == true
            ? await _repository.GetPagedAtivosAsync(request.PageNumber, request.PageSize)
            : await _repository.GetPagedAsync(request.PageNumber, request.PageSize);

        var items = pagedResult.Items.Select(p => new ProdutoDto
        {
            Id = p.Id,
            Nome = p.Nome,
            Descricao = p.Descricao,
            Preco = p.Preco,
            QuantidadeEstoque = p.QuantidadeEstoque,
            Ativo = p.Ativo,
            CriadoEm = p.CriadoEm,
            AtualizadoEm = p.AtualizadoEm
        });

        return PagedResult<ProdutoDto>.Create(
            items,
            pagedResult.PageNumber,
            pagedResult.PageSize,
            pagedResult.TotalCount
        );
    }
}
