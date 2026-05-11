using Microsoft.EntityFrameworkCore;
using MyWebshop.ConsoleApp.Models;

namespace MyWebshop.ConsoleApp.DAL;

public class WebShopDbContext : DbContext
{
    public DbSet<Customer> Customers { get; set; }

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

        modelBuilder.Entity<Customer>()
            .HasKey(c => c.Id);

        modelBuilder.Entity<Customer>()
            .Property(c => c.Name)
            .HasMaxLength(100)
            .IsRequired();

        modelBuilder.Entity<Customer>()
            .Property(c => c.PhoneNumber)
            .HasMaxLength(20)
            .IsRequired(false);

        modelBuilder.Entity<Customer>()
            .Property(c => c.CreditLimit)
            .HasColumnType("decimal(18,2)")
            .HasPrecision(18, 2);     // 👈 niet echt meer nodig, puur voor andere databases ...

        modelBuilder.Entity<Customer>()
            .Property(c => c.CreatedAt)
            .HasColumnType("datetime2(0)")
            .HasDefaultValueSql("SYSDATETIME()")   // 👈 default constraint
            .IsRequired();

    }
}
