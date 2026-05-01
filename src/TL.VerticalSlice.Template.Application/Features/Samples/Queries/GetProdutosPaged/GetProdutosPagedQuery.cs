using MediatR;
using TL.VerticalSlice.Template.Application.Common.Models;
using TL.VerticalSlice.Template.Application.Contracts.Repositories;

namespace TL.VerticalSlice.Template.Application.Features.Samples.Queries.GetSamplesPaged;

// â”€â”€ Query â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
public record GetSamplesPagedQuery(
    int PageNumber = 1,
    int PageSize = 20,
    bool? ApenasAtivos = null
) : IRequest<PagedResult<SampleDto>>;

// â”€â”€ Handler â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
public class GetSamplesPagedQueryHandler : IRequestHandler<GetSamplesPagedQuery, PagedResult<SampleDto>>
{
    private readonly ISampleRepository _repository;

    public GetSamplesPagedQueryHandler(ISampleRepository repository)
        => _repository = repository;

    public async Task<PagedResult<SampleDto>> Handle(
        GetSamplesPagedQuery request,
        CancellationToken cancellationToken)
    {
        var pagedResult = request.ApenasAtivos == true
            ? await _repository.GetPagedAtivosAsync(request.PageNumber, request.PageSize)
            : await _repository.GetPagedAsync(request.PageNumber, request.PageSize);

        var items = pagedResult.Items.Select(p => new SampleDto
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

        return PagedResult<SampleDto>.Create(
            items,
            pagedResult.PageNumber,
            pagedResult.PageSize,
            pagedResult.TotalCount
        );
    }
}

