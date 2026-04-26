using FluentValidation;
using MediatR;
using TL.Exemplo.Application.Common.Models;
using TL.Exemplo.Application.Contracts.Repositories;

namespace TL.Exemplo.Application.Features.Produtos.Queries.GetAllProdutos;

// ── Query ──────────────────────────────────────────────────────────────────
public record GetAllProdutosQuery(bool? ApenasAtivos = null) : IRequest<IEnumerable<ProdutoDto>>;

// ── Validator ──────────────────────────────────────────────────────────────
public class GetAllProdutosQueryValidator : AbstractValidator<GetAllProdutosQuery>
{
    public GetAllProdutosQueryValidator()
    {
        // Exemplo de validação para queries de listagem
        // Neste caso não há parâmetros obrigatórios, mas o validator está pronto para extensão.
    }
}

// ── Handler ────────────────────────────────────────────────────────────────
public class GetAllProdutosQueryHandler : IRequestHandler<GetAllProdutosQuery, IEnumerable<ProdutoDto>>
{
    private readonly IProdutoRepository _repository;

    public GetAllProdutosQueryHandler(IProdutoRepository repository)
        => _repository = repository;

    public async Task<IEnumerable<ProdutoDto>> Handle(
        GetAllProdutosQuery request,
        CancellationToken cancellationToken)
    {
        var produtos = request.ApenasAtivos.HasValue && request.ApenasAtivos.Value
            ? await _repository.GetAllAtivosAsync()
            : await _repository.GetAllAsync();

        return produtos.Select(p => new ProdutoDto
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
    }
}
