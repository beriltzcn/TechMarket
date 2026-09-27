namespace TechMarket.Web.Models.ViewModels
{
    public class ProductListViewModel
    {
        public List<Product> Products { get; set; } = new();

        public string? Search { get; set; }
        public string? Sort { get; set; }
        public string? Category { get; set; }

        // Every category that exists in the catalogue - fills the filter list.
        public List<string> Categories { get; set; } = new();

        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages{ get; set; }

        public bool HasPrevious => PageIndex > 1;
        public bool HasNext => PageIndex < TotalPages;
    }
}
