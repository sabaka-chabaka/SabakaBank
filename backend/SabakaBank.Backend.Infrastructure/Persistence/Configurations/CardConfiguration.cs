using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SabakaBank.Backend.Domain.Entities;

namespace SabakaBank.Backend.Infrastructure.Persistence.Configurations;

public class CardConfiguration : IEntityTypeConfiguration<Card>
{
    public void Configure(EntityTypeBuilder<Card> builder)
    {
        builder.ToTable("cards");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .HasColumnName("id");

        builder.Property(c => c.CardNumber)
            .HasColumnName("card_number")
            .HasMaxLength(19)
            .IsRequired();

        builder.Property(c => c.CvvHash)
            .HasColumnName("cvv_hash")
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(c => c.HolderName)
            .HasColumnName("holder_name")
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(c => c.ExpiresAt)
            .HasColumnName("expires_at")
            .IsRequired();

        builder.Property(c => c.Type)
            .HasColumnName("type")
            .HasConversion<string>()
            .IsRequired();

        builder.Property(c => c.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .IsRequired();

        builder.Property(c => c.AccountId)
            .HasColumnName("account_id")
            .IsRequired();

        builder.Property(c => c.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(c => c.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        builder.HasIndex(c => c.CardNumber).IsUnique();
    }
}