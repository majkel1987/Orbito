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
        /// Page number (1-based)
        /// </summary>
        public int PageNumber { get; set; } = 1;

        /// <summary>
        /// Number of items per page
        /// </summary>
        public int PageSize { get; set; } = DefaultPageSize;

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
        /// Validate and normalize pagination parameters
        /// </summary>
        /// <param name="securityLimitService">Security limit service for max page size validation</param>
        public void Validate(ISecurityLimitService? securityLimitService = null)
        {
            if (PageNumber < 1)
                PageNumber = 1;

            if (PageSize < MinPageSize)
                PageSize = DefaultPageSize;

            // Use ISecurityLimitService if available, otherwise fallback to hardcoded 100
            var maxPageSize = securityLimitService?.MaxPageSize ?? 100;
            if (PageSize > maxPageSize)
                PageSize = maxPageSize;
        }
    }
}
