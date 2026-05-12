namespace MyWebshop.ConsoleApp.Models;

public class Order
{
    public int Id { get; set; }
    public DateTime OrderDate { get; set; }
    public decimal TotalAmount { get; set; }

    public int CustomerId { get; set; }
    // Navigation property to Customer (geen required gebruiken)
    public Customer Customer { get; set; } = null!;

}
