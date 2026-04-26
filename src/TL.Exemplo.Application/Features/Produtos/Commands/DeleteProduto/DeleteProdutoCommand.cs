using FluentValidation;
using MediatR;
using TL.Exemplo.Application.Common.Exceptions;
using TL.Exemplo.Application.Contracts.Repositories;

namespace TL.Exemplo.Application.Features.Produtos.Commands.DeleteProduto;

// ── Command ────────────────────────────────────────────────────────────────
public record DeleteProdutoCommand(int Id) : IRequest<Unit>;

// ── Validator ──────────────────────────────────────────────────────────────
public class DeleteProdutoCommandValidator : AbstractValidator<DeleteProdutoCommand>
{
    public DeleteProdutoCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("O Id deve ser maior que zero.");
    }
}

// ── Handler ────────────────────────────────────────────────────────────────
public class DeleteProdutoCommandHandler : IRequestHandler<DeleteProdutoCommand, Unit>
{
    private readonly IProdutoRepository _repository;

    public DeleteProdutoCommandHandler(IProdutoRepository repository)
        => _repository = repository;

    public async Task<Unit> Handle(
        DeleteProdutoCommand request,
        CancellationToken cancellationToken)
    {
        var produto = await _repository.GetByIdAsync(request.Id)
            ?? throw new NotFoundException(nameof(Domain.Entities.Produto), request.Id);

        await _repository.DeleteAsync(produto.Id);

        return Unit.Value;
    }
}
