namespace Orbito.Application.Common.Models
{
    /// <summary>
    /// Generic paginated list for handling paginated results
    /// </summary>
    /// <typeparam name="T">Type of items in the list</typeparam>
    public class PaginatedList<T>
    {
        /// <summary>
        /// Items in the current page
        /// </summary>
        public List<T> Items { get; init; } = new();

        /// <summary>
        /// Total number of items across all pages
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// Current page number (1-based)
        /// </summary>
        public int PageNumber { get; set; }

        /// <summary>
        /// Number of items per page
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// Total number of pages
        /// </summary>
        public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)TotalCount / PageSize) : 0;

        /// <summary>
        /// Whether there is a previous page
        /// </summary>
        public bool HasPreviousPage => PageNumber > 1;

        /// <summary>
        /// Whether there is a next page
        /// </summary>
        public bool HasNextPage => PageNumber < TotalPages;

        /// <summary>
        /// Default constructor
        /// </summary>
        public PaginatedList()
        {
        }

        /// <summary>
        /// Constructor with parameters
        /// </summary>
        /// <param name="items">Items for the current page</param>
        /// <param name="totalCount">Total number of items</param>
        /// <param name="pageNumber">Current page number</param>
        /// <param name="pageSize">Number of items per page</param>
        public PaginatedList(List<T> items, int totalCount, int pageNumber, int pageSize)
        {
            Items = items;
            TotalCount = totalCount;
            PageNumber = pageNumber;
            PageSize = pageSize;
        }

        /// <summary>
        /// Create an empty paginated list
        /// </summary>
        /// <param name="pageNumber">Page number</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>Empty paginated list</returns>
        public static PaginatedList<T> Empty(int pageNumber = 1, int pageSize = 10)
        {
            return new PaginatedList<T>(new List<T>(), 0, pageNumber, pageSize);
        }
    }
}
