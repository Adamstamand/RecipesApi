using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RecipesInfrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddedPrivacyLevelToRecipe : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Privacy",
                table: "Recipes",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Privacy",
                table: "Recipes");
        }
    }
}
