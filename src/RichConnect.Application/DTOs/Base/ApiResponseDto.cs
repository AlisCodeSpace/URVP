namespace RICHConnect.Backend.DTOs
{
    /// <summary>
    /// Standardized API response wrapper
    /// </summary>
    /// <typeparam name="T">Type of the data payload</typeparam>
    public class ApiResponseDto<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
        public List<string> Errors { get; set; } = new();
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string? TraceId { get; set; }

        public static ApiResponseDto<T> SuccessResult(T data, string message = "Operation completed successfully")
        {
            return new ApiResponseDto<T>
            {
                Success = true,
                Message = message,
                Data = data
            };
        }

        public static ApiResponseDto<T> ErrorResult(string message, List<string>? errors = null)
        {
            return new ApiResponseDto<T>
            {
                Success = false,
                Message = message,
                Errors = errors ?? new List<string>()
            };
        }
    }

    /// <summary>
    /// Paginated response wrapper
    /// </summary>
    /// <typeparam name="T">Type of the data items</typeparam>
    public class PaginatedResponseDto<T>
    {
        public List<T> Items { get; set; } = new();
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
        public bool HasPreviousPage { get; set; }
        public bool HasNextPage { get; set; }

        public static PaginatedResponseDto<T> Create(List<T> items, int pageNumber, int pageSize, int totalCount)
        {
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            
            return new PaginatedResponseDto<T>
            {
                Items = items,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = totalPages,
                HasPreviousPage = pageNumber > 1,
                HasNextPage = pageNumber < totalPages
            };
        }
    }

    /// <summary>
    /// Pagination parameters for API requests
    /// </summary>
    public class PaginationParameters
    {
        private int _pageNumber = 1;
        private int _pageSize = 10;
        private const int MaxPageSize = 100;

        public int PageNumber
        {
            get => _pageNumber;
            set => _pageNumber = value < 1 ? 1 : value;
        }

        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = value > MaxPageSize ? MaxPageSize : value < 1 ? 1 : value;
        }

        public string? SortBy { get; set; }
        public bool SortDescending { get; set; } = false;
        public string? SearchTerm { get; set; }
    }

    /// <summary>
    /// Audit log entry DTO
    /// </summary>
    public class AuditLogDto
    {
        public Guid Id { get; set; }
        public string ActionType { get; set; } = string.Empty;
        public string EntityType { get; set; } = string.Empty;
        public Guid EntityId { get; set; }
        public string? OldValues { get; set; }
        public string? NewValues { get; set; }
        public Guid AdminUserId { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? ClientIpHash { get; set; }
    }
}
