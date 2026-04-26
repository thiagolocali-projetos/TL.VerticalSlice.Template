using FluentValidation;
using MediatR;
using TL.Exemplo.Application.Common.Models;
using TL.Exemplo.Domain.Entities;
using TL.Exemplo.Application.Contracts.Repositories;

namespace TL.Exemplo.Application.Features.Produtos.Commands.CreateProduto;

// ── Command ────────────────────────────────────────────────────────────────
public record CreateProdutoCommand(
    string Nome,
    string Descricao,
    decimal Preco,
    int QuantidadeEstoque
) : IRequest<ProdutoDto>;

// ── Validator ──────────────────────────────────────────────────────────────
public class CreateProdutoCommandValidator : AbstractValidator<CreateProdutoCommand>
{
    public CreateProdutoCommandValidator()
    {
        RuleFor(x => x.Nome)
            .NotEmpty().WithMessage("O nome é obrigatório.")
            .MinimumLength(3).WithMessage("O nome deve ter no mínimo 3 caracteres.")
            .MaximumLength(150).WithMessage("O nome deve ter no máximo 150 caracteres.");

        RuleFor(x => x.Descricao)
            .NotEmpty().WithMessage("A descrição é obrigatória.")
            .MaximumLength(500).WithMessage("A descrição deve ter no máximo 500 caracteres.");

        RuleFor(x => x.Preco)
            .GreaterThan(0).WithMessage("O preço deve ser maior que zero.")
            .LessThanOrEqualTo(999999.99m).WithMessage("O preço não pode exceder R$ 999.999,99.");

        RuleFor(x => x.QuantidadeEstoque)
            .GreaterThanOrEqualTo(0).WithMessage("A quantidade em estoque não pode ser negativa.");
    }
}

// ── Handler ────────────────────────────────────────────────────────────────
public class CreateProdutoCommandHandler : IRequestHandler<CreateProdutoCommand, ProdutoDto>
{
    private readonly IProdutoRepository _repository;

    public CreateProdutoCommandHandler(IProdutoRepository repository)
        => _repository = repository;

    public async Task<ProdutoDto> Handle(
        CreateProdutoCommand request,
        CancellationToken cancellationToken)
    {
        var produto = new Produto
        {
            Nome = request.Nome,
            Descricao = request.Descricao,
            Preco = request.Preco,
            QuantidadeEstoque = request.QuantidadeEstoque,
            Ativo = true,
            CriadoEm = DateTime.UtcNow
        };

        var id = await _repository.CreateAsync(produto);
        produto.Id = id;

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
