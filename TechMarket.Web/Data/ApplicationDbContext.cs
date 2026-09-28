using Microsoft.EntityFrameworkCore;
using TechMarket.Web.Models;

namespace TechMarket.Web.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Product> Products => Set<Product>();

        public DbSet<ProductSpecification> ProductSpecifications => Set<ProductSpecification>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Product>(entity =>
            {
                entity.Property(p => p.Price).HasColumnType("decimal(18,2)");
                entity.HasIndex(p => p.Name);
            });

            modelBuilder.Entity<ProductSpecification>(entity =>
            {
                // One product has many specifications.
                // Deleting a product also deletes its specifications.
                entity.HasOne(s => s.Product)
                      .WithMany(p => p.Specifications)
                      .HasForeignKey(s => s.ProductId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}