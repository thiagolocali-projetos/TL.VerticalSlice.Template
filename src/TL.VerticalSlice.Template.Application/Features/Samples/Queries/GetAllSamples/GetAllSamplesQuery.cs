using FluentValidation;
using MediatR;
using TL.VerticalSlice.Template.Application.Common.Models;
using TL.VerticalSlice.Template.Application.Contracts.Repositories;

namespace TL.VerticalSlice.Template.Application.Features.Samples.Queries.GetAllSamples;

// â”€â”€ Query â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
public record GetAllSamplesQuery(bool? ApenasAtivos = null) : IRequest<IEnumerable<SampleDto>>;

// â”€â”€ Validator â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
public class GetAllSamplesQueryValidator : AbstractValidator<GetAllSamplesQuery>
{
    public GetAllSamplesQueryValidator()
    {
        // Exemplo de validaÃ§Ã£o para queries de listagem
        // Neste caso nÃ£o hÃ¡ parÃ¢metros obrigatÃ³rios, mas o validator estÃ¡ pronto para extensÃ£o.
    }
}

// â”€â”€ Handler â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
public class GetAllSamplesQueryHandler : IRequestHandler<GetAllSamplesQuery, IEnumerable<SampleDto>>
{
    private readonly ISampleRepository _repository;

    public GetAllSamplesQueryHandler(ISampleRepository repository)
        => _repository = repository;

    public async Task<IEnumerable<SampleDto>> Handle(
        GetAllSamplesQuery request,
        CancellationToken cancellationToken)
    {
        var Samples = request.ApenasAtivos.HasValue && request.ApenasAtivos.Value
            ? await _repository.GetAllAtivosAsync()
            : await _repository.GetAllAsync();

        return Samples.Select(p => new SampleDto
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

