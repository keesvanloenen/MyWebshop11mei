namespace MyWebshop.ConsoleApp.Models;

public class Category
{
    public int Id { get; set; }
    public required string Name { get; set; }

    // Navigation property for many-to-many relationship
    public ICollection<Product> Products { get; } = [];
}
