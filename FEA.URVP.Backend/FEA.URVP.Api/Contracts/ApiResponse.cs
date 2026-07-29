namespace FEA.URVP.Api.Contracts;

/// <summary>
/// Standardized API response envelope.
/// </summary>
public class ApiResponse<T>
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
    public T? Data { get; init; }
    public IReadOnlyList<string> Errors { get; init; } = [];
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public string? TraceId { get; set; }

    public static ApiResponse<T> SuccessResult(T data, string message = "Operation completed successfully") =>
        new()
        {
            Success = true,
            Message = message,
            Data = data
        };

    public static ApiResponse<T> ErrorResult(string message, IEnumerable<string>? errors = null) =>
        new()
        {
            Success = false,
            Message = message,
            Errors = errors?.ToList() ?? []
        };
}

/// <summary>
/// Paginated response envelope.
/// </summary>
public class PaginatedResponse<T>
{
    public IReadOnlyList<T> Items { get; init; } = [];
    public int PageNumber { get; init; }
    public int PageSize { get; init; }
    public int TotalCount { get; init; }
    public int TotalPages { get; init; }
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;

    public static PaginatedResponse<T> Create(
        IReadOnlyList<T> items,
        int pageNumber,
        int pageSize,
        int totalCount)
    {
        var totalPages = pageSize <= 0 ? 0 : (int)Math.Ceiling(totalCount / (double)pageSize);

        return new PaginatedResponse<T>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = totalPages
        };
    }
}
