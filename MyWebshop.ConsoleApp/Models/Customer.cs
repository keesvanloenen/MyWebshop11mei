namespace MyWebshop.ConsoleApp.Models;

public class Customer
{
    
    public int Id { get; set; }

    public required string Name { get; set; }

    public string? PhoneNumber { get; set; }

    public decimal CreditLimit { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    // Concurrency token (SQL Server specifiek / database beheerd)
    public byte[] RowVersion { get; set; } = null!;

    // Navigation property to Orders (one-to-many)
    public ICollection<Order> Orders { get; set; } = [];
}
