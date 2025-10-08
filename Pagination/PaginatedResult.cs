using RbacApi.QueryFilters;

namespace RbacApi.Pagination
{
    public class PaginatedResult<TModel> where TModel : class
    {
        public int Count { get; private set; }
        public int PageIndex { get; private set; }
        public int PageSize { get; private set; }
        public int PageCount { get; private set; }
        public int ResultsCount { get; private set; }
        public bool HasPreviousPage => PageIndex > 1;
        public int? PreviousPageNumber => HasPreviousPage ? PageIndex - 1 : null;
        public bool HasNextPage => PageIndex < PageCount;
        public int? NextPageNumber => HasNextPage ? PageIndex + 1 : null;
        public IEnumerable<TModel> Results { get; private set; } = [];


        private PaginatedResult(IEnumerable<TModel> results, PaginationQueryFilter filter, int count)
        {
            Count = count;
            PageIndex = filter.PageIndex ?? 1;
            PageSize = filter.PageSize ?? 10;
            PageCount = (int)Math.Ceiling(count / (double)PageSize);
            ResultsCount = results.Count();
            Results = results;
        }

        public static PaginatedResult<TModel> Create
            (IEnumerable<TModel> results, PaginationQueryFilter filter, int count)
        {
            return new PaginatedResult<TModel>(results, filter, count);
        }
    }
}
