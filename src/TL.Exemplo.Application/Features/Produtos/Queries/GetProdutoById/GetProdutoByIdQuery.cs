using FluentValidation;
using MediatR;
using TL.Exemplo.Application.Common.Exceptions;
using TL.Exemplo.Application.Common.Models;
using TL.Exemplo.Application.Contracts.Repositories;

namespace TL.Exemplo.Application.Features.Produtos.Queries.GetProdutoById;

// ── Query ──────────────────────────────────────────────────────────────────
public record GetProdutoByIdQuery(int Id) : IRequest<ProdutoDto>;

// ── Validator ──────────────────────────────────────────────────────────────
public class GetProdutoByIdQueryValidator : AbstractValidator<GetProdutoByIdQuery>
{
    public GetProdutoByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("O Id deve ser maior que zero.");
    }
}

// ── Handler ────────────────────────────────────────────────────────────────
public class GetProdutoByIdQueryHandler : IRequestHandler<GetProdutoByIdQuery, ProdutoDto>
{
    private readonly IProdutoRepository _repository;

    public GetProdutoByIdQueryHandler(IProdutoRepository repository)
        => _repository = repository;

    public async Task<ProdutoDto> Handle(
        GetProdutoByIdQuery request,
        CancellationToken cancellationToken)
    {
        var produto = await _repository.GetByIdAsync(request.Id)
            ?? throw new NotFoundException(nameof(Domain.Entities.Produto), request.Id);

        return new ProdutoDto
        {
            Id = produto.Id,
            Nome = produto.Nome,
            Descricao = produto.Descricao,
            Preco = produto.Preco,
            QuantidadeEstoque = produto.QuantidadeEstoque,
            Ativo = produto.Ativo,
            CriadoEm = produto.CriadoEm,
            AtualizadoEm = produto.AtualizadoEm
        };
    }
}
