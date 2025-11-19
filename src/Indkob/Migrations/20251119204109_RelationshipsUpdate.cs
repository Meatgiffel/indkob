using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Indkob.Migrations
{
    /// <inheritdoc />
    public partial class RelationshipsUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GroceryListItems_GroceryCategories_GroceryCategoryId",
                table: "GroceryListItems");

            migrationBuilder.DropForeignKey(
                name: "FK_GroceryListItems_GroceryItems_GroceryItemId",
                table: "GroceryListItems");

            migrationBuilder.DropForeignKey(
                name: "FK_MealPlanEntries_Meals_MealId",
                table: "MealPlanEntries");

            migrationBuilder.AddForeignKey(
                name: "FK_GroceryListItems_GroceryCategories_GroceryCategoryId",
                table: "GroceryListItems",
                column: "GroceryCategoryId",
                principalTable: "GroceryCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_GroceryListItems_GroceryItems_GroceryItemId",
                table: "GroceryListItems",
                column: "GroceryItemId",
                principalTable: "GroceryItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_MealPlanEntries_Meals_MealId",
                table: "MealPlanEntries",
                column: "MealId",
                principalTable: "Meals",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GroceryListItems_GroceryCategories_GroceryCategoryId",
                table: "GroceryListItems");

            migrationBuilder.DropForeignKey(
                name: "FK_GroceryListItems_GroceryItems_GroceryItemId",
                table: "GroceryListItems");

            migrationBuilder.DropForeignKey(
                name: "FK_MealPlanEntries_Meals_MealId",
                table: "MealPlanEntries");

            migrationBuilder.AddForeignKey(
                name: "FK_GroceryListItems_GroceryCategories_GroceryCategoryId",
                table: "GroceryListItems",
                column: "GroceryCategoryId",
                principalTable: "GroceryCategories",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_GroceryListItems_GroceryItems_GroceryItemId",
                table: "GroceryListItems",
                column: "GroceryItemId",
                principalTable: "GroceryItems",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_MealPlanEntries_Meals_MealId",
                table: "MealPlanEntries",
                column: "MealId",
                principalTable: "Meals",
                principalColumn: "Id");
        }
    }
}
