using Microsoft.EntityFrameworkCore;
using TechMarket.Web.Models;

namespace TechMarket.Web.Data
{
    /// <summary>
    /// Writes the starting catalogue once. If the table already has rows,
    /// it does nothing - so it is safe to run on every startup.
    /// </summary>
    public static class DbSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext db)
        {
            // If the table already has rows, there is nothing to do.
            if (await db.Products.AnyAsync())
            {
                return;
            }

            db.Products.AddRange(
                new Product
                {
                    Name = "Apple MacBook Air 13 M4",
                    Brand = "Apple",
                    Category = "Laptops",
                    Price = 49999m,
                    Stock = 25,
                    Description = "13.6 inch Liquid Retina display, M4 chip, 16 GB memory, 256 GB SSD."
                },
                new Product
                {
                    Name = "Samsung Galaxy S25 Ultra 256 GB",
                    Brand = "Samsung",
                    Category = "Smartphones",
                    Price = 68499m,
                    Stock = 35,
                    Description = "6.8 inch Dynamic AMOLED display, Snapdragon 8 Elite, 200 MP camera."
                },
                new Product
                {
                    Name = "LG OLED evo C5 55 inch",
                    Brand = "LG",
                    Category = "Televisions",
                    Price = 74999m,
                    Stock = 10,
                    Description = "4K OLED panel, 144 Hz refresh rate, Dolby Atmos sound."
                },
                new Product
                {
                    Name = "Sony WH-1000XM6 Wireless Headphones",
                    Brand = "Sony",
                    Category = "Headphones",
                    Price = 15999m,
                    Stock = 45,
                    Description = "Industry leading active noise cancelling, up to 30 hours battery."
                },
                new Product
                {
                    Name = "PlayStation 5 Pro 2 TB",
                    Brand = "Sony",
                    Category = "Game Consoles",
                    Price = 44999m,
                    Stock = 0,
                    Description = "2 TB SSD, up to 120 fps, includes one DualSense controller."
                },
                new Product
                {
                    Name = "Asus ROG Strix G16 Gaming Laptop",
                    Brand = "Asus",
                    Category = "Laptops",
                    Price = 69999m,
                    Stock = 12,
                    Description = "16 inch 240 Hz display, RTX 4060 graphics, 16 GB DDR5 memory."
                }
            );

            await db.SaveChangesAsync();
        }
    }
}