using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyWebshop.ConsoleApp.Models;

public class Customer
{
    [Key]
    public int Id { get; set; }

    //[Column(TypeName= "uniqueidentifier")]
    //public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(100)]
    public required string Name { get; set; }

    [MaxLength(20)]
    public string? PhoneNumber { get; set; }

    [Column(TypeName = "decimal(8,2)")]
    public decimal CreditLimit { get; set; }

    [Column(TypeName = "datetime2(0)")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
