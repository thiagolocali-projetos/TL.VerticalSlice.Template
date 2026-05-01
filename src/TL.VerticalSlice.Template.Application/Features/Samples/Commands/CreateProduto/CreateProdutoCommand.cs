using FluentValidation;
using MediatR;
using TL.VerticalSlice.Template.Application.Common.Models;
using TL.VerticalSlice.Template.Domain.Entities;
using TL.VerticalSlice.Template.Application.Contracts.Repositories;

namespace TL.VerticalSlice.Template.Application.Features.Samples.Commands.CreateSample;

// â”€â”€ Command â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
public record CreateSampleCommand(
    string Nome,
    string Descricao,
    decimal Preco,
    int QuantidadeEstoque
) : IRequest<SampleDto>;

// â”€â”€ Validator â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
public class CreateSampleCommandValidator : AbstractValidator<CreateSampleCommand>
{
    public CreateSampleCommandValidator()
    {
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
public class CreateSampleCommandHandler : IRequestHandler<CreateSampleCommand, SampleDto>
{
    private readonly ISampleRepository _repository;

    public CreateSampleCommandHandler(ISampleRepository repository)
        => _repository = repository;

    public async Task<SampleDto> Handle(
        CreateSampleCommand request,
        CancellationToken cancellationToken)
    {
        var Sample = new Sample
        {
            Nome = request.Nome,
            Descricao = request.Descricao,
            Preco = request.Preco,
            QuantidadeEstoque = request.QuantidadeEstoque,
            Ativo = true,
            CriadoEm = DateTime.UtcNow
        };

        var id = await _repository.CreateAsync(Sample);
        Sample.Id = id;

        return new SampleDto
        {
            Id = Sample.Id,
            Nome = Sample.Nome,
            Descricao = Sample.Descricao,
            Preco = Sample.Preco,
            QuantidadeEstoque = Sample.QuantidadeEstoque,
            Ativo = Sample.Ativo,
            CriadoEm = Sample.CriadoEm,
            AtualizadoEm = Sample.AtualizadoEm
        };
    }
}

