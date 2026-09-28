using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechMarket.Web.Data;
using TechMarket.Web.Models;
using TechMarket.Web.Models.ViewModels;

namespace TechMarket.Web.Controllers
{
    public class ProductController : Controller
    {
        // Only these extensions are accepted.
        private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp" };

        // 2 MB
        private const long MaxFileSize = 2 * 1024 * 1024;

        private readonly ApplicationDbContext _db;
        private readonly IWebHostEnvironment _env;

        public ProductController(ApplicationDbContext db, IWebHostEnvironment env)
        {
            _db = db;
            _env = env;
        }

        public async Task<IActionResult> Delete(int id)
        {
            var product = await _db.Products.FindAsync(id);

            if(product is null)
            {
                return NotFound();
            }
            return View(product);

        }


        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _db.Products.FindAsync(id);

            if(product is null)
            {
                return NotFound();
            }

            DeleteImage(product.ImageUrl);

            _db.Products.Remove(product);
            await _db.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Details(int id)
        {
            var product = await _db.Products
                .Include(p => p.Specifications)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product is null)
            {
                return NotFound();
            }

            var relatedProducts = await _db.Products
                .Where(p => p.Category == product.Category && p.Id != product.Id)
                .OrderByDescending(p => p.Price)
                .Take(3)
                .ToListAsync();

            var viewModel = new ProductDetailsViewModel
            {
                Product = product,
                RelatedProducts = relatedProducts,
            };

            return View(viewModel);
        }

        public async Task<IActionResult> Index(string? q, string? sort, string? category, int page = 1)
        {
            const int pageSize = 4;
            IQueryable<Product> query = _db.Products;

            if(!string.IsNullOrWhiteSpace(q))
            {
                
                var term = q.Trim().ToLowerInvariant();
                query = query.Where(p => p.Name.ToLower().Contains(term) || p.Brand.ToLower().Contains(term));
            }

            if (!string.IsNullOrWhiteSpace(category))
            {
                query = query.Where(p => p.Category == category);
            }

            query = sort switch
            {
                "name_desc" => query.OrderByDescending(p => p.Name),
                "price_asc" => query.OrderBy(p => p.DiscountPrice ?? p.Price),
                "price_desc" => query.OrderByDescending(p => p.DiscountPrice ?? p.Price),
                _ => query.OrderBy(p => p.Name),
            };

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            if (page < 1)
            {
                page = 1;
            }
            if (totalPages > 0 && page > totalPages)
            {
                page = totalPages;
            }
            var product = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var viewModel = new ProductListViewModel
            {
                Products = product,
                Search = q,
                Sort = sort,
                PageIndex = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = totalPages,
                Category = category,
                Categories = await _db.Products
                    .Select(p => p.Category)
                    .Distinct()
                    .OrderBy(c => c)
                    .ToListAsync(),
            };

            return View(viewModel);
        }


        // GET: /Product/Create - shows an empty form
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Product/Create - receives the submitted form
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product)
        {
            if (ModelState.IsValid)
            {
                var imageUrl= await SaveImageAsync(product.ImageFile);

                if (imageUrl is not null)
                {
                    product.ImageUrl = imageUrl;
                }
            }

            // SaveImageAsync may have added validation errors, so check again.
            if (!ModelState.IsValid)
            {
                return View(product);
            }

            _db.Products.Add(product);
            await _db.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Edit(int id)
        {
            var product = await _db.Products
                .Include(p => p.Specifications)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product is null)
            {
                return NotFound();
            }

            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Product product)
        { 
            if(id != product.Id)
            {
                return NotFound();
            }

            if(!ModelState.IsValid)
            {
                return View(product);
            }

            var existing = await _db.Products.FindAsync(id);

            if (existing is null)
            {
                return NotFound();
            }

            // A new image was uploaded? Save it and delete the old file.
            var newImageUrl = await SaveImageAsync(product.ImageFile);

            if (newImageUrl is not null)
            {
                DeleteImage(existing.ImageUrl);
                existing.ImageUrl = newImageUrl;
            }

            // Copy the form values onto the tracked entity.
            existing.Name = product.Name;
            existing.Brand = product.Brand;
            existing.Category = product.Category;
            existing.Price = product.Price;
            existing.DiscountPrice = product.DiscountPrice;
            existing.Stock = product.Stock;
            existing.Description = product.Description;

            await _db.SaveChangesAsync();

            return RedirectToAction("Details", new { id = existing.Id });

        }

        // POST: /Product/AddSpecification
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddSpecification(int productId, string name, string value)
        {
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(value))
            {
                TempData["SpecError"] = "Both the specification name and its value are required.";
                return RedirectToAction("Edit", new { id = productId });
            }

            var productExists = await _db.Products.AnyAsync(p => p.Id == productId);
            if (!productExists)
            {
                return NotFound();
            }

            _db.ProductSpecifications.Add(new ProductSpecification
            {
                ProductId = productId,
                Name = name.Trim(),
                Value = value.Trim(),
            });

            await _db.SaveChangesAsync();

            return RedirectToAction("Edit", new { id = productId });
        }

        // POST: /Product/DeleteSpecification
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteSpecification(int id)
        {
            var specification = await _db.ProductSpecifications.FindAsync(id);

            if (specification is null)
            {
                return NotFound();
            }

            var productId = specification.ProductId;

            _db.ProductSpecifications.Remove(specification);
            await _db.SaveChangesAsync();

            return RedirectToAction("Edit", new { id = productId });
        }

        /// <summary>
        /// Saves the uploaded file under wwwroot/images/products
        /// and returns its public web path. Returns null when there is no file.
        /// </summary>
        private async Task<string?> SaveImageAsync(IFormFile? file)
        {
            if (file is null || file.Length == 0)
            {
                return null;
            }

            if (file.Length > MaxFileSize)
            {
                ModelState.AddModelError(nameof(Product.ImageFile), "The image must be 2 MB or smaller.");
                return null;
            }

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(extension))
            {
                ModelState.AddModelError(nameof(Product.ImageFile), "Only .jpg, .jpeg, .png and .webp files are allowed.");
                return null;
            }

            // Never trust the file name that comes from the browser.
            var fileName = $"{Guid.NewGuid():N}{extension}";

            var folder = Path.Combine(_env.WebRootPath, "images", "products");
            Directory.CreateDirectory(folder);

            var fullPath = Path.Combine(folder, fileName);

            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return $"/images/products/{fileName}";
        }

        // Removes a previously uploaded image file from disk.
        private void DeleteImage(string? imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl))
            {
                return;
            }

            // Take only the file name, so a crafted path cannot escape the folder.
            var fileName = Path.GetFileName(imageUrl);
            var fullPath = Path.Combine(_env.WebRootPath, "images", "products", fileName);

            if (System.IO.File.Exists(fullPath))
            {
                System.IO.File.Delete(fullPath);
            }
        }
    }
}
