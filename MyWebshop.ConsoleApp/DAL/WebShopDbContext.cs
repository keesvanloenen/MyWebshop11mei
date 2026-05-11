using Microsoft.EntityFrameworkCore;
using MyWebshop.ConsoleApp.DAL.Configurations;
using MyWebshop.ConsoleApp.Models;

namespace MyWebshop.ConsoleApp.DAL;

public class WebShopDbContext : DbContext
{
    public DbSet<Customer> Customers { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<PhysicalProduct> PhysicalProducts { get; set; }
    public DbSet<DigitalProduct> DigitalProducts { get; set; }

    public WebShopDbContext(DbContextOptions<WebShopDbContext> options) : base(options)
    {
    }

    //protected override void OnConfiguring(DbContextOptionsBuilder builder)
    //{
    //    builder.UseSqlServer(@"Server=(localdb)\mssqllocaldb;Database=MyWebshop;ConnectRetryCount=0");
    //}

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration<Customer>(new CustomerConfiguration());

        modelBuilder.Entity<Product>().UseTpcMappingStrategy();

    }
}
