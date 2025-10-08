namespace RbacApi.QueryFilters
{
    public class ProductQueryFilter : PaginationQueryFilter
    {
        public string? Name { get; set; }
        public string? Category { get; set; }
        public string? Subcategory { get; set; }
    }
}
