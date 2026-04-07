using Orbito.Application.Common.Interfaces;

namespace Orbito.Application.Common.Models
{
    /// <summary>
    /// Parameters for pagination
    /// </summary>
    public class PaginationParams
    {
        private const int DefaultPageSize = 10;
        private const int MinPageSize = 1;

        /// <summary>
        /// Maximum allowed page size to prevent performance issues.
        /// Used across all paginated queries for consistency.
        /// </summary>
        public const int MaxPageSize = 100;

        /// <summary>
        /// Page number (1-based)
        /// </summary>
        public int PageNumber { get; init; } = 1;

        /// <summary>
        /// Number of items per page
        /// </summary>
        public int PageSize { get; init; } = DefaultPageSize;

        /// <summary>
        /// Default constructor
        /// </summary>
        public PaginationParams()
        {
        }

        /// <summary>
        /// Constructor with parameters
        /// </summary>
        /// <param name="pageNumber">Page number</param>
        /// <param name="pageSize">Page size</param>
        public PaginationParams(int pageNumber, int pageSize)
        {
            PageNumber = pageNumber;
            PageSize = pageSize;
        }

        /// <summary>
        /// Validate and normalize pagination parameters using default max page size
        /// </summary>
        /// <returns>New normalized PaginationParams instance</returns>
        public PaginationParams Validate()
        {
            return Validate(MaxPageSize);
        }

        /// <summary>
        /// Validate and normalize pagination parameters with custom max page size
        /// </summary>
        /// <param name="maxPageSize">Maximum allowed page size</param>
        /// <returns>New normalized PaginationParams instance</returns>
        public PaginationParams Validate(int maxPageSize)
        {
            var normalizedPageNumber = PageNumber < 1 ? 1 : PageNumber;
            var normalizedPageSize = PageSize < MinPageSize ? DefaultPageSize : PageSize;

            if (normalizedPageSize > maxPageSize)
                normalizedPageSize = maxPageSize;

            return new PaginationParams(normalizedPageNumber, normalizedPageSize);
        }

        /// <summary>
        /// Validate and normalize pagination parameters using security limit service
        /// </summary>
        /// <param name="securityLimitService">Security limit service for max page size validation</param>
        /// <returns>New normalized PaginationParams instance</returns>
        public PaginationParams ValidateWithService(ISecurityLimitService securityLimitService)
        {
            ArgumentNullException.ThrowIfNull(securityLimitService);
            return Validate(securityLimitService.MaxPageSize);
        }
    }
}
