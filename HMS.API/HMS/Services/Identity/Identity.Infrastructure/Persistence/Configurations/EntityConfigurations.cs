using HMS.Identity.Domain.Entities;
using HMS.Shared.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HMS.Identity.Infrastructure.Persistence.Configurations;

public sealed class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("Roles");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name).HasMaxLength(50).IsRequired();
        builder.Property(x => x.NormalizedName).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(200);

        builder.HasIndex(x => x.Name).IsUnique();
        builder.HasIndex(x => x.NormalizedName).IsUnique();
    }
}

public sealed class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
{
    public void Configure(EntityTypeBuilder<UserRole> builder)
    {
        builder.ToTable("UserRoles");
        builder.HasKey(x => x.Id);

        builder.HasIndex(x => new { x.UserId, x.RoleId }).IsUnique();

        builder.HasOne(x => x.Role)
            .WithMany()
            .HasForeignKey(x => x.RoleId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("RefreshTokens");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Token).HasMaxLength(500).IsRequired();
        builder.HasIndex(x => x.Token).IsUnique();
        builder.HasIndex(x => x.UserId);
    }
}

public sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("OutboxEvents");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Type).HasMaxLength(150).IsRequired();
        builder.Property(x => x.Payload).IsRequired();
        builder.Property(x => x.LastError).HasMaxLength(500);

        builder.HasIndex(x => new { x.Status, x.OccurredOn })
            .HasFilter("[Status] = 0");
    }
}