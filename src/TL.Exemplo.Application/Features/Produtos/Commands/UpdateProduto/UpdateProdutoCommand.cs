using FluentValidation;
using MediatR;
using TL.Exemplo.Application.Common.Exceptions;
using TL.Exemplo.Application.Contracts.Repositories;

namespace TL.Exemplo.Application.Features.Produtos.Commands.UpdateProduto;

// ── Command ────────────────────────────────────────────────────────────────
public record UpdateProdutoCommand(
    int Id,
    string Nome,
    string Descricao,
    decimal Preco,
    int QuantidadeEstoque,
    bool Ativo
) : IRequest<Unit>;

// ── Validator ──────────────────────────────────────────────────────────────
public class UpdateProdutoCommandValidator : AbstractValidator<UpdateProdutoCommand>
{
    public UpdateProdutoCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("O Id deve ser maior que zero.");

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
public class UpdateProdutoCommandHandler : IRequestHandler<UpdateProdutoCommand, Unit>
{
    private readonly IProdutoRepository _repository;

    public UpdateProdutoCommandHandler(IProdutoRepository repository)
        => _repository = repository;

    public async Task<Unit> Handle(
        UpdateProdutoCommand request,
        CancellationToken cancellationToken)
    {
        var produto = await _repository.GetByIdAsync(request.Id)
            ?? throw new NotFoundException(nameof(Domain.Entities.Produto), request.Id);

        produto.Nome = request.Nome;
        produto.Descricao = request.Descricao;
        produto.Preco = request.Preco;
        produto.QuantidadeEstoque = request.QuantidadeEstoque;
        produto.Ativo = request.Ativo;
        produto.AtualizadoEm = DateTime.UtcNow;

        await _repository.UpdateAsync(produto);

        return Unit.Value;
    }
}
