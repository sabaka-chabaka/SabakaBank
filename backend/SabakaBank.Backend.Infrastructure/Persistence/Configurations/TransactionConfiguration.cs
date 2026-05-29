using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SabakaBank.Backend.Domain.Entities;

namespace SabakaBank.Backend.Infrastructure.Persistence.Configurations;

public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.ToTable("transactions");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Id)
            .HasColumnName("id");

        builder.Property(t => t.Type)
            .HasColumnName("type")
            .HasConversion<string>()
            .IsRequired();

        builder.Property(t => t.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .IsRequired();

        builder.Property(t => t.Amount)
            .HasColumnName("amount")
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(t => t.Currency)
            .HasColumnName("currency")
            .HasConversion<string>()
            .IsRequired();

        builder.Property(t => t.ConvertedAmount)
            .HasColumnName("converted_amount")
            .HasPrecision(18, 2);

        builder.Property(t => t.ConvertedCurrency)
            .HasColumnName("converted_currency")
            .HasConversion<string>();

        builder.Property(t => t.Description)
            .HasColumnName("description")
            .HasMaxLength(500);

        builder.Property(t => t.FromAccountId)
            .HasColumnName("from_account_id")
            .IsRequired();

        builder.Property(t => t.ToAccountId)
            .HasColumnName("to_account_id");

        builder.Property(t => t.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(t => t.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        builder.HasOne(t => t.FromAccount)
            .WithMany(a => a.Transactions)
            .HasForeignKey(t => t.FromAccountId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.ToAccount)
            .WithMany()
            .HasForeignKey(t => t.ToAccountId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(t => t.FromAccountId);
        builder.HasIndex(t => t.ToAccountId);
        builder.HasIndex(t => t.CreatedAt);
    }
}