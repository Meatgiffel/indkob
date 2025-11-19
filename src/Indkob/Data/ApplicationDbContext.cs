using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Indkob.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Meal> Meals => Set<Meal>();
    public DbSet<MealPlanEntry> MealPlanEntries => Set<MealPlanEntry>();
    public DbSet<GroceryCategory> GroceryCategories => Set<GroceryCategory>();
    public DbSet<GroceryItem> GroceryItems => Set<GroceryItem>();
    public DbSet<GroceryList> GroceryLists => Set<GroceryList>();
    public DbSet<GroceryListItem> GroceryListItems => Set<GroceryListItem>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<MealPlanEntry>()
            .HasIndex(p => new { p.Day, p.Slot })
            .IsUnique();

        builder.Entity<MealPlanEntry>()
            .HasOne(e => e.Meal)
            .WithMany()
            .HasForeignKey(e => e.MealId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<GroceryListItem>()
            .HasOne(i => i.GroceryList)
            .WithMany(l => l.Items)
            .HasForeignKey(i => i.GroceryListId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<GroceryListItem>()
            .HasOne(i => i.GroceryItem)
            .WithMany()
            .HasForeignKey(i => i.GroceryItemId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<GroceryListItem>()
            .HasOne(i => i.GroceryCategory)
            .WithMany()
            .HasForeignKey(i => i.GroceryCategoryId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
