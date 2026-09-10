namespace Services.DTOs.Shared
{
    public class PagedResult<T>
    {
        public IEnumerable<T> Items { get; set; } = [];
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
        public bool HasNextPage => Page < TotalPages;
        public bool HasPreviousPage => Page > 1;
        public int? NextPage => HasNextPage ? Page + 1 : null;
        public int? PreviousPage => HasPreviousPage ? Page - 1 : null;

        public static PagedResult<T> Create(IEnumerable<T> items, int totalCount, PagedRequest request)
            => new()
            {
                Items = items,
                TotalCount = totalCount,
                Page = request.Page,
                PageSize = request.PageSize
            };
    }
}
