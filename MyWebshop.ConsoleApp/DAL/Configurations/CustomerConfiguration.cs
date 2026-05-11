using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyWebshop.ConsoleApp.Models;
using System.Reflection.Emit;

namespace MyWebshop.ConsoleApp.DAL.Configurations;

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder
            .HasKey(c => c.Id);

        builder
            .Property(c => c.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder
            .Property(c => c.PhoneNumber)
            .HasMaxLength(20)
            .IsRequired(false);

        builder
            .Property(c => c.CreditLimit)
            .HasColumnType("decimal(18,2)")
            .HasPrecision(18, 2);     // 👈 niet echt meer nodig, puur voor andere databases ...

        builder
            .Property(c => c.CreatedAt)
            .HasColumnType("datetime2(0)")
            .HasDefaultValueSql("SYSDATETIME()")   // 👈 default constraint
            .IsRequired();
    }
}
