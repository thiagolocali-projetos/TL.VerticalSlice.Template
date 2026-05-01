using FluentValidation;
using MediatR;
using TL.VerticalSlice.Template.Application.Common.Exceptions;
using TL.VerticalSlice.Template.Application.Contracts.Repositories;

namespace TL.VerticalSlice.Template.Application.Features.Samples.Commands.UpdateSample;

// â”€â”€ Command â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
public record UpdateSampleCommand(
    int Id,
    string Nome,
    string Descricao,
    decimal Preco,
    int QuantidadeEstoque,
    bool Ativo
) : IRequest<Unit>;

// â”€â”€ Validator â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
public class UpdateSampleCommandValidator : AbstractValidator<UpdateSampleCommand>
{
    public UpdateSampleCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("O Id deve ser maior que zero.");

        RuleFor(x => x.Nome)
            .NotEmpty().WithMessage("O nome Ã© obrigatÃ³rio.")
            .MinimumLength(3).WithMessage("O nome deve ter no mÃ­nimo 3 caracteres.")
            .MaximumLength(150).WithMessage("O nome deve ter no mÃ¡ximo 150 caracteres.");

        RuleFor(x => x.Descricao)
            .NotEmpty().WithMessage("A descriÃ§Ã£o Ã© obrigatÃ³ria.")
            .MaximumLength(500).WithMessage("A descriÃ§Ã£o deve ter no mÃ¡ximo 500 caracteres.");

        RuleFor(x => x.Preco)
            .GreaterThan(0).WithMessage("O preÃ§o deve ser maior que zero.")
            .LessThanOrEqualTo(999999.99m).WithMessage("O preÃ§o nÃ£o pode exceder R$ 999.999,99.");

        RuleFor(x => x.QuantidadeEstoque)
            .GreaterThanOrEqualTo(0).WithMessage("A quantidade em estoque nÃ£o pode ser negativa.");
    }
}

// â”€â”€ Handler â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
public class UpdateSampleCommandHandler : IRequestHandler<UpdateSampleCommand, Unit>
{
    private readonly ISampleRepository _repository;

    public UpdateSampleCommandHandler(ISampleRepository repository)
        => _repository = repository;

    public async Task<Unit> Handle(
        UpdateSampleCommand request,
        CancellationToken cancellationToken)
    {
        var Sample = await _repository.GetByIdAsync(request.Id)
            ?? throw new NotFoundException(nameof(Domain.Entities.Sample), request.Id);

        Sample.Nome = request.Nome;
        Sample.Descricao = request.Descricao;
        Sample.Preco = request.Preco;
        Sample.QuantidadeEstoque = request.QuantidadeEstoque;
        Sample.Ativo = request.Ativo;
        Sample.AtualizadoEm = DateTime.UtcNow;

        await _repository.UpdateAsync(Sample);

        return Unit.Value;
    }
}

