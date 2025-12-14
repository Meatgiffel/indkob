using Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Item> Items => Set<Item>();
    public DbSet<GroceryEntry> GroceryEntries => Set<GroceryEntry>();
    public DbSet<MealPlanDay> MealPlanDays => Set<MealPlanDay>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.Property(e => e.UserName).IsRequired();
            entity.Property(e => e.NormalizedUserName).IsRequired();
            entity.Property(e => e.PasswordHash).IsRequired();
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .ValueGeneratedOnAdd();
            entity.HasIndex(e => e.NormalizedUserName).IsUnique();
        });

        modelBuilder.Entity<Item>(entity =>
        {
            entity.Property(e => e.Name).IsRequired();
            entity.Property(e => e.Area).IsRequired();
        });

        modelBuilder.Entity<GroceryEntry>(entity =>
        {
            entity.Property(e => e.Amount).HasMaxLength(64);
            entity.Property(e => e.Note).HasMaxLength(512);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .ValueGeneratedOnAdd();

            entity.HasOne(e => e.Item)
                .WithMany(i => i.GroceryEntries)
                .HasForeignKey(e => e.ItemId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<MealPlanDay>(entity =>
        {
            entity.Property(e => e.Date).IsRequired();
            entity.Property(e => e.Dinner).HasMaxLength(256);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .ValueGeneratedOnAdd();
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .ValueGeneratedOnAddOrUpdate();
            entity.HasIndex(e => e.Date).IsUnique();
        });
    }
}
