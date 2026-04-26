using TL.Exemplo.Domain.Entities;

namespace TL.Exemplo.Application.Contracts.Repositories;

/// <summary>
/// Contrato do repositório de produtos.
/// Definido na Application para que os handlers não dependam da Infrastructure.
/// A implementação concreta fica na Infrastructure.
/// </summary>
public interface IProdutoRepository
{
    Task<IEnumerable<Produto>> GetAllAsync();
    Task<IEnumerable<Produto>> GetAllAtivosAsync();
    Task<Produto?> GetByIdAsync(int id);
    Task<int> CreateAsync(Produto produto);
    Task UpdateAsync(Produto produto);
    Task DeleteAsync(int id);
}
