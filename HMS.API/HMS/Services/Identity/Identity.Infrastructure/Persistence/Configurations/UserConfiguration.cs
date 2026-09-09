using Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(x => x.Id);

        // Basic
        builder.Property(x => x.Username).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Email).HasMaxLength(100).IsRequired();
        builder.Property(x => x.PasswordHash).HasMaxLength(255).IsRequired();
        builder.Property(x => x.PasswordSalt).HasMaxLength(255).IsRequired();
        builder.Property(x => x.FullName).HasMaxLength(150).IsRequired();
        builder.Property(x => x.PhoneNumber).HasMaxLength(20);

        // Account status
        builder.Property(x => x.IsActive).IsRequired();
        builder.Property(x => x.IsLockedOut).IsRequired();
        builder.Property(x => x.AccessFailedCount).IsRequired();

        // Lockout
        builder.Property(x => x.LockoutEndDate);

        // Login tracking
        builder.Property(x => x.LastLoginDate);
        builder.Property(x => x.LastLoginIp).HasMaxLength(45);

        // Password reset
        builder.Property(x => x.PasswordResetToken).HasMaxLength(500);
        builder.Property(x => x.PasswordResetTokenExpiresAtUtc);

        // Audit
        builder.Property(x => x.CreatedDate).IsRequired();
        builder.Property(x => x.CreatedBy).HasMaxLength(100);
        builder.Property(x => x.ModifiedDate).HasMaxLength(100);
        builder.Property(x => x.DeletedDate);
        builder.Property(x => x.ModifiedBy).HasMaxLength(100);
        builder.Property(x => x.IsDeleted).IsRequired();

        // Index
        builder.HasIndex(x => x.Username).IsUnique();
        builder.HasIndex(x => x.Email).IsUnique();

        // Soft delete
        builder.HasQueryFilter(x => !x.IsDeleted);

        // Roles
        builder.HasMany(x => x.Roles).WithOne(r => r.User).HasForeignKey(r => r.UserId).OnDelete(DeleteBehavior.Cascade);

        // Refresh Tokens
        builder.HasMany(x => x.RefreshTokens).WithOne(t => t.User).HasForeignKey(t => t.UserId).OnDelete(DeleteBehavior.Cascade);
    }
}