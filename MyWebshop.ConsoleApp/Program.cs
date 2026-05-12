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

        //ShowCustomers(options);          // basic
        //ShowCustomerForMarly(options);   // querying
        //ShowProducts(options);           // inheritance
        //ShowOrders(options);             // one-to-many
        //ShowCategories(options);         // many-to-many (explicit)
        //Crud(options);
        //EagerLoading(options);
        //ExplicitLoading(options);
        OptimisticConcurrency(options);
    }

    private static void OptimisticConcurrency(DbContextOptions<WebShopDbContext> options)
    {
        using var context = new WebShopDbContext(options);

        // User A
        var customer = context.Customers.Find(1)!;
        Console.WriteLine($"User A loads: Credit = {customer.CreditLimit}");

        // User B
        context.Customers
            .Where(c => c.Id == 1)
            .ExecuteUpdate(setters => setters.SetProperty(c => c.CreditLimit, 2000m));
        Console.WriteLine($"User B saved: Credit = 2000");

        try
        {
            // User A
            customer.CreditLimit = 1500m;
            context.SaveChanges();
        } catch (DbUpdateConcurrencyException ex)
        {
            Console.WriteLine("⚠️ CONFLICT");

            foreach (var entry in ex.Entries)
            {
                if (entry.Entity is Customer conflictedCustomer)
                {
                    // Get current database values
                    var dbValues = entry.GetDatabaseValues()!;

                    // DB WINS scenario
                    //entry.CurrentValues.SetValues(dbValues);
                    //Console.WriteLine($"❌ Changes discarded {conflictedCustomer.CreditLimit}");

                    // CLIENT WINS scenario
                    entry.OriginalValues.SetValues(dbValues);
                    context.SaveChanges();
                    Console.WriteLine($"✔️ Client wins: {conflictedCustomer.CreditLimit:C} saved");
                }
            }
            
        }
    }

    private static void Crud(DbContextOptions<WebShopDbContext> options)
    {
        int newCustomerId = 0;      // hulvariabele

        using (var context = new WebShopDbContext(options))
        {
            var newCustomer = new Customer
            {
                Name = "Gijs",
                PhoneNumber = "1234567890",
                CreditLimit = 1000.00m,
                CreatedAt = DateTime.Now,
            };

            ((List<Order>)newCustomer.Orders).AddRange([
                new Order{
                    OrderDate = DateTime.Now.AddDays(-2),
                    TotalAmount = 149.99m
                },
                new Order{
                    OrderDate = DateTime.Now,
                    TotalAmount = 249.99m
                },
            ]);

            context.Customers.Add(newCustomer);

            context.SaveChanges();
            newCustomerId = newCustomer.Id;
        }

        Console.WriteLine(newCustomerId);

        using (var context = new WebShopDbContext(options))
        {
            var customer = context.Customers
                .Include(c => c.Orders)
                .FirstOrDefault(c => c.Id == newCustomerId);

            if (customer is not null)
            {
                customer.CreditLimit += 1_000_000m;

                var firstOrder = customer.Orders.First();
                firstOrder.TotalAmount += 2_000_000m;

                context.ChangeTracker.DetectChanges();
                Console.WriteLine(context.ChangeTracker.DebugView.LongView);
                context.SaveChanges();
            }
        }

        using (var context = new WebShopDbContext(options))
        {
            var gijs = context.Customers.Find(7)!;

            context.Customers.Remove(gijs);

            context.SaveChanges();
        }
    }

    private static void ExplicitLoading(DbContextOptions<WebShopDbContext> options)
    {
        Console.Write("Vul het customerid in: ");
        string input = Console.ReadLine() ?? string.Empty;

        int customerId = int.Parse(input);

        using var context = new WebShopDbContext(options);

        Customer? customer = context.Customers.Find(customerId);

        if (customer is null)
        {
            Console.WriteLine($"Customer met id {customerId} niet gevonden!");
            return;
        }

        //context.Entry(customer)
        //    .Collection(c => c.Orders)
        //    .Load();

        //context.Entry(customer)
        //    .Collection(c => c.Orders)
        //    .Query()
        //    .Take(1)
        //    .ToList();

        var orders = context.Orders
            .AsNoTracking()
            .Where(o => o.CustomerId == customerId)
            .Select(o => new
            {
                o.Id,
                o.OrderDate,
                o.TotalAmount,
            }).Take(1);

        Console.WriteLine($"Orders voor Customer {customerId}");

        foreach (var order in orders) 
        {
            Console.WriteLine($"\t - [{order.Id}] {order.OrderDate:dd-MM-yyyy}: {order.TotalAmount:C}");
        }

    }

    private static void ShowCustomers(DbContextOptions<WebShopDbContext> options)
    {
        using var context = new WebShopDbContext(options);

        var customers = context.Customers.Select(c => c);

        customers = customers.Where(c => c.Name.Length > 4);
        Console.WriteLine(customers.ToQueryString());



        foreach (var customer in customers)
        {
            Console.WriteLine($"{customer.Id} {customer.Name}: {customer.CreatedAt:yyyy-MM-dd HH:mm:ss}");
        }
    }

    private static void ShowCustomerForMarly(DbContextOptions<WebShopDbContext> options)
    {
        using var context = new WebShopDbContext(options);

        // 🔍 FirstOrDefault: found entity or null
        Customer? customer1 = context.Customers.FirstOrDefault(c => c.Id == 10);
        Console.WriteLine($"Found: {customer1?.Name ?? "nothing"}");

        //if (customer1 is null)
        //{
        //    Console.WriteLine("No customer found with id 10");
        //    return;
        //}


        // 🔍 First: found entity or exception
        Customer customer2 = context.Customers.First(c => c.Id == 2);
        Console.WriteLine($"Found: {customer2.Name}");
        // Change 2 into 10 leads to exception

        // 🔍 SingleOrDefault: found entity or null (exception when more than 1)
        Customer? customer3 = context.Customers.SingleOrDefault(c => c.Name.Length == 6);
        Console.WriteLine($"Found: {customer3?.Name ?? "nothing"}");
        // Change 6 into 2 leads to exception!

        // 🔍 Single: exception when not found + exception when more than 1
        Customer customer4 = context.Customers.Single(c => c.Name.Length == 6);
        Console.WriteLine($"Found: {customer4.Name}");

        // 🔍 Find: like FirstOrDefault, but with only an identifier!
        // Works on context.Customers
        // First checks if ChangeTracker has the entity aboard, otherwise database is queried
        Customer? customer5 = context.Customers.Find(5);
        Console.WriteLine($"Found: {customer5?.Name ?? "nothing"}");
    }

    private static void ShowCategories(DbContextOptions<WebShopDbContext> options)
    {
        using var context = new WebShopDbContext(options);

        //Console.WriteLine("===== Show per product the categorie(s) =====\n");
        //var productCategories = context.ProductCategories
        //    .Include(pc => pc.Product)
        //    .Include(pc => pc.Category);

        //foreach (var pc in productCategories)
        //{
        //    Console.WriteLine($"{pc.Category.Name} - {pc.Product.Name} ({pc.AddedOn})");
        //}

        var products = context.Products
            .AsNoTracking()
            .Include(p => p.ProductCategories)
            .ThenInclude(pc => pc.Category)
            .AsSplitQuery();

        Console.WriteLine(products.ToQueryString());
        foreach (var product in products)
        {
            Console.WriteLine(product.Name);

            foreach (var productCategory in product.ProductCategories)
            {
                Console.WriteLine($"\t- {productCategory.Category.Name}");
            }

        }


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
            }
            else if (product is DigitalProduct dp)
            {
                Console.WriteLine($" | Size: {dp.FileSizeInMb} Mb");
            }
        }
    }

    private static void ShowOrders(DbContextOptions<WebShopDbContext> options)
    {
        using var context = new WebShopDbContext(options);

        var orders = context.Orders
            .Include(o => o.Customer);  // later more 👊 eager loading
        

        Console.WriteLine(orders.ToQueryString());

        foreach (var order in orders) 
        {
            Console.WriteLine($"{order.OrderDate} - {order.Customer.Name}");
        }
    }

    private static void EagerLoading(DbContextOptions<WebShopDbContext> options)
    {
        using var context = new WebShopDbContext(options);

        // Dit is een filtered include
        var customers = context.Customers
            .AsNoTracking()
            .Include(
                c => c.Orders
                        .OrderByDescending(o => o.OrderDate)
                        .Take(1)
            );

        Console.WriteLine(customers.ToQueryString());

        foreach (var customer in customers)
        {
            Console.WriteLine($"{customer.Name}");

            foreach (var order in customer.Orders)
            {
                Console.WriteLine($"\t {order.OrderDate} {order.TotalAmount}");
            }
        }
    }

    private static void DataSeed(DbContextOptions<WebShopDbContext> options)
    {
        using var context = new WebShopDbContext(options);

        var customer1 = new Customer { Name = "Ab" };
        var customer2 = new Customer { Name = "Bo" };
        var customer3 = new Customer { Name = "Cas" };
        var customer4 = new Customer { Name = "Dik" };
        var customer5 = new Customer { Name = "Eduard" };
        var customer6 = new Customer { Name = "Fe" };

        customer1.Orders.Add(new Order { OrderDate = DateTime.Now.AddDays(-4), TotalAmount = 450.00m });
        customer1.Orders.Add(new Order { OrderDate = DateTime.Now.AddDays(-7), TotalAmount = 190.00m });
        customer2.Orders.Add(new Order { OrderDate = DateTime.Now.AddDays(-1), TotalAmount = 27.50m });


        context.Customers.AddRange([customer1, customer2, customer3, customer4, customer5, customer6]);
        context.SaveChanges();

        var physicalProduct1 = new PhysicalProduct { Name = "Laptop", Price = 999.99m, Weight = 1.5m };
        var physicalProduct2 = new PhysicalProduct { Name = "Mouse", Price = 19.99m, Weight = 0.1m };
        var digitalProduct1 = new DigitalProduct { Name = "C# for Dummies", Price = 9.99m, FileSizeInMb = 5 };
        var digitalProduct2 = new DigitalProduct { Name = "LINQ Course", Price = 49.99m, FileSizeInMb = 1200 };

        context.Products.AddRange([physicalProduct1, physicalProduct2, digitalProduct1, digitalProduct2]);

        context.Products.AddRange([physicalProduct1, physicalProduct2, digitalProduct1, digitalProduct2]);

        var electronics = new Category { Name = "Electronics" };
        var software = new Category { Name = "Software" };
        var accessories = new Category { Name = "Accessories" };

        context.Categories.AddRange([electronics, software, accessories]);

        context.ProductCategories.AddRange([
           new ProductCategory { Product = physicalProduct1, Category = electronics, AddedOn = DateTime.Now.AddDays(-10) },
           new ProductCategory { Product = physicalProduct2, Category = electronics, AddedOn = DateTime.Now.AddDays(-1) },
           new ProductCategory { Product = physicalProduct2, Category = accessories, AddedOn = DateTime.Now.AddDays(-22) },
           new ProductCategory { Product = digitalProduct1, Category = software, AddedOn = DateTime.Now.AddDays(-7) },
           new ProductCategory { Product = digitalProduct2, Category = software, AddedOn = DateTime.Now.AddDays(-5) },
        ]);

        context.SaveChanges();
    }

    private static void Initialize(DbContextOptions<WebShopDbContext> options)
    {
        using var context = new WebShopDbContext(options);

        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();       // quick prototyping
    }
}
