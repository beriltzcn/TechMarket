using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using TechMarket.Web.Data;
using TechMarket.Web.Models;

namespace TechMarket.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _db;

        // The framework injects the database context through this constructor.
        public HomeController(ApplicationDbContext db)
        {
            _db = db;
        }

        // Home page: / - shows only the three newest products
        public async Task<IActionResult> Index()
        {
            var latestProducts = await _db.Products
                .OrderByDescending(p => p.Id)
                .Take(3)
                .ToListAsync();

            return View(latestProducts);
        }

        // Privacy page: /Home/Privacy
        public IActionResult Privacy()
        {
            return View();
        }

        // Contact page: /Home/Contact
        public IActionResult Contact()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
} 