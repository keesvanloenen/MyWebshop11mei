using Microsoft.EntityFrameworkCore;
using MyWebshop.ConsoleApp.DAL;
using MyWebshop.ConsoleApp.Models;

namespace MyWebshop.ConsoleApp;

internal class Program
{
    static void Main(string[] args)
    {
        var options = new DbContextOptionsBuilder<WebShopDbContext>()
            .UseSqlServer(@"Server=(localdb)\mssqllocaldb;Database=MyWebshop;ConnectRetryCount=0")
            .Options;

        Initialize(options);
        DataSeed(options);
        ShowCustomers(options);
    }

    private static void DataSeed(DbContextOptions<WebShopDbContext> options)
    {
        using var context = new WebShopDbContext(options);

        var customer1 = new Customer { Name = "Ab" };
        var customer2 = new Customer { Name = "Bo" };
        var customer3 = new Customer { Name = "Cas" };

        context.Customers.AddRange([customer1, customer2, customer3]);
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
            Console.WriteLine($"{customer.Id} {customer.Name}");
        }
    }
}
