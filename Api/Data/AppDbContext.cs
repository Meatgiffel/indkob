using Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Item> Items => Set<Item>();
    public DbSet<GroceryEntry> GroceryEntries => Set<GroceryEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
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
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
