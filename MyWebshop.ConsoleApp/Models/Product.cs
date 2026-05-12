namespace MyWebshop.ConsoleApp.Models;

public abstract class Product
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public decimal Price { get; set; }

    // Navigation property for many-to-many relationship
    public ICollection<ProductCategory> ProductCategories { get; } = [];
}
