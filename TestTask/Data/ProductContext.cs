using Microsoft.EntityFrameworkCore;
using TestTask.Models;

namespace TestTask.Data;

public class ProductContext : DbContext
{
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer("Server=.;Database=ProductDB;Trusted_Connection=true;TrustServerCertificate=True;");
    }

    public DbSet<TvProducts> TvProducts { get; set; }
    public DbSet<DslProducts> DslProducts { get; set; }
    public DbSet<Customer> Customers { get; set; }
}
