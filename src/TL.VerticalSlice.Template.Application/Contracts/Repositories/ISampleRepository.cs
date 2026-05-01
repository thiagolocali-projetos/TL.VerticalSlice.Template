using TL.VerticalSlice.Template.Application.Common.Models;
using TL.VerticalSlice.Template.Domain.Entities;

namespace TL.VerticalSlice.Template.Application.Contracts.Repositories;

/// <summary>
/// Contrato do repositÃ³rio de Samples.
/// Definido na Application para que os handlers nÃ£o dependam da Infrastructure.
/// A implementaÃ§Ã£o concreta fica na Infrastructure.
/// </summary>
public interface ISampleRepository
{
    Task<IEnumerable<Sample>> GetAllAsync();
    Task<IEnumerable<Sample>> GetAllAtivosAsync();
    Task<Sample?> GetByIdAsync(int id);
    Task<int> CreateAsync(Sample Sample);
    Task UpdateAsync(Sample Sample);
    Task DeleteAsync(int id);

    // MÃ©todos de paginaÃ§Ã£o
    Task<PagedResult<Sample>> GetPagedAsync(int pageNumber = 1, int pageSize = 20);
    Task<PagedResult<Sample>> GetPagedAtivosAsync(int pageNumber = 1, int pageSize = 20);
}

