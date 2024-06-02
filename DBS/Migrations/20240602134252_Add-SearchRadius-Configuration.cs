using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DBS.Migrations
{
    /// <inheritdoc />
    public partial class AddSearchRadiusConfiguration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SearchRadius_Distance",
                table: "PriceConfigurations",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SearchRadius_Unit",
                table: "PriceConfigurations",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SearchRadius_Distance",
                table: "PriceConfigurations");

            migrationBuilder.DropColumn(
                name: "SearchRadius_Unit",
                table: "PriceConfigurations");
        }
    }
}
