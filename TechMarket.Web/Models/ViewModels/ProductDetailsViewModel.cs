namespace TechMarket.Web.Models.ViewModels
{
    /// <summary>
    /// Everything the details page needs: the product itself
    /// plus the other products from the same category.
    /// </summary>
    public class ProductDetailsViewModel
    {
        public required Product Product { get; set; }

        public List<Product> RelatedProducts { get; set; } = new();
    }
}
