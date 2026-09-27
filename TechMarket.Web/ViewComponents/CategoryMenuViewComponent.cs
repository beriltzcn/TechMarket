using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechMarket.Web.Data;

namespace TechMarket.Web.ViewComponents
{
    /// <summary>
    /// A reusable piece that fetches its own data.
    /// The layout calls it, so the category bar is ready on every page.
    /// </summary>
    public class CategoryMenuViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _db;

        public CategoryMenuViewComponent(ApplicationDbContext db) => _db = db;

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var categories = await _db.Products
                .Select(p => p.Category)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();

            return View(categories);
        }
    }
}
