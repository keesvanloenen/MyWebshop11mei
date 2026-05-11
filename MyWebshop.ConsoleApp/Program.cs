using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MyWebshop.ConsoleApp.DAL;
using MyWebshop.ConsoleApp.Models;
using System.Text;

namespace MyWebshop.ConsoleApp;

internal class Program
{
    static void Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;     // to display € (and emojis)

        var builder = new ConfigurationBuilder().AddJsonFile("appsettings.json");
        var config = builder.Build();

        var options = new DbContextOptionsBuilder<WebShopDbContext>()
            .UseSqlServer(config.GetConnectionString("DefaultConn"))
            .Options;

        Initialize(options);
        DataSeed(options);

        //ShowCustomers(options);
        ShowProducts(options);
    }

    private static void ShowProducts(DbContextOptions<WebShopDbContext> options)
    {
        using var context = new WebShopDbContext(options);

        var products = context.Products;

        foreach (var product in products)
        {
            Console.Write($"{product.Id} {product.Name} - {product.Price:C}");

            if (product is PhysicalProduct pp)
            {
                Console.WriteLine($" | Weight: {pp.Weight} kg");
            } else if (product is DigitalProduct dp)
            {
                Console.WriteLine($" | Size: {dp.FileSizeInMb} Mb");
            }
        }
    }

    private static void DataSeed(DbContextOptions<WebShopDbContext> options)
    {
        using var context = new WebShopDbContext(options);

        var customer1 = new Customer { Name = "Ab" };
        var customer2 = new Customer { Name = "Bo" };
        var customer3 = new Customer { Name = "Cas" };

        context.Customers.AddRange([customer1, customer2, customer3]);
        context.SaveChanges();

        var physicalProduct1 = new PhysicalProduct { Name = "Laptop", Price = 999.99m, Weight = 1.5m };
        var physicalProduct2 = new PhysicalProduct { Name = "Mouse", Price = 19.99m, Weight = 0.1m };
        var digitalProduct1 = new DigitalProduct { Name = "C# for Dummies", Price = 9.99m, FileSizeInMb = 5 };
        var digitalProduct2 = new DigitalProduct { Name = "LINQ Course", Price = 49.99m, FileSizeInMb = 1200 };

        context.Products.AddRange([physicalProduct1, physicalProduct2, digitalProduct1, digitalProduct2]);
        context.SaveChanges();


    }

    private static void Initialize(DbContextOptions<WebShopDbContext> options)
    {
        using var context = new WebShopDbContext(options);

        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();       // quick prototyping
    }

    private static void ShowCustomers(DbContextOptions<WebShopDbContext> options)
    {
        using var context = new WebShopDbContext(options);

        var customers = context.Customers;

        foreach (var customer in customers)
        {
            Console.WriteLine($"{customer.Id} {customer.Name}: {customer.CreatedAt:yyyy-MM-dd HH:mm:ss}");
        }
    }
}
