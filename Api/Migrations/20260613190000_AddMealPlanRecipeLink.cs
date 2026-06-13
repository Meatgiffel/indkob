using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class AddMealPlanRecipeLink : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RecipeSlug",
                table: "MealPlanDays",
                type: "TEXT",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RecipeName",
                table: "MealPlanDays",
                type: "TEXT",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RecipeId",
                table: "MealPlanDays",
                type: "TEXT",
                maxLength: 64,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RecipeSlug",
                table: "MealPlanDays");

            migrationBuilder.DropColumn(
                name: "RecipeName",
                table: "MealPlanDays");

            migrationBuilder.DropColumn(
                name: "RecipeId",
                table: "MealPlanDays");
        }
    }
}
