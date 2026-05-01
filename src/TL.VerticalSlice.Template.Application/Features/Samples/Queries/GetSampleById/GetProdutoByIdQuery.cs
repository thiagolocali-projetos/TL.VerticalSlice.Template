using FluentValidation;
using MediatR;
using TL.VerticalSlice.Template.Application.Common.Exceptions;
using TL.VerticalSlice.Template.Application.Common.Models;
using TL.VerticalSlice.Template.Application.Contracts.Repositories;

namespace TL.VerticalSlice.Template.Application.Features.Samples.Queries.GetSampleById;

// â”€â”€ Query â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
public record GetSampleByIdQuery(int Id) : IRequest<SampleDto>;

// â”€â”€ Validator â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
public class GetSampleByIdQueryValidator : AbstractValidator<GetSampleByIdQuery>
{
    public GetSampleByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("O Id deve ser maior que zero.");
    }
}

// â”€â”€ Handler â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
public class GetSampleByIdQueryHandler : IRequestHandler<GetSampleByIdQuery, SampleDto>
{
    private readonly ISampleRepository _repository;

    public GetSampleByIdQueryHandler(ISampleRepository repository)
        => _repository = repository;

    public async Task<SampleDto> Handle(
        GetSampleByIdQuery request,
        CancellationToken cancellationToken)
    {
        var Sample = await _repository.GetByIdAsync(request.Id)
            ?? throw new NotFoundException(nameof(Domain.Entities.Sample), request.Id);

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

