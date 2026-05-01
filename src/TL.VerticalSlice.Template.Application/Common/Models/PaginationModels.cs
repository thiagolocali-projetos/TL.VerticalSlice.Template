namespace TL.VerticalSlice.Template.Application.Common.Models;

/// <summary>
/// ParÃ¢metros de paginaÃ§Ã£o para requisiÃ§Ãµes.
/// </summary>
public class PaginationParams
{
    private int _pageNumber = 1;
    private int _pageSize = 20;

    public const int MaxPageSize = 100;
    public const int MinPageNumber = 1;

    public int PageNumber
    {
        get => _pageNumber;
        set => _pageNumber = value < MinPageNumber ? MinPageNumber : value;
    }

    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value > MaxPageSize ? MaxPageSize : value < 1 ? 1 : value;
    }
}

/// <summary>
/// Resultado paginado contendo dados e metadados de paginaÃ§Ã£o.
/// </summary>
public record PagedResult<T>(
    IEnumerable<T> Items,
    int PageNumber,
    int PageSize,
    int TotalCount,
    int TotalPages
)
{
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;

    public static PagedResult<T> Create(
        IEnumerable<T> items,
        int pageNumber,
        int pageSize,
        int totalCount)
    {
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        return new PagedResult<T>(items, pageNumber, pageSize, totalCount, totalPages);
    }
}

