namespace RbacApi.QueryFilters
{
    public abstract class PaginationQueryFilter
    {
        public SortOrder? Sort { get; set; }
        public int? PageIndex { get; set; }
        public int? PageSize { get; set; }
    }

    public enum SortOrder
    {
        Ascending,
        Descending
    }
}
