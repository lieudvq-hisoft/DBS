using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DBS.Migrations
{
    /// <inheritdoc />
    public partial class AddFieldDriver : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DeclineRequest",
                table: "AspNetUsers",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TotalRequest",
                table: "AspNetUsers",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeclineRequest",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "TotalRequest",
                table: "AspNetUsers");
        }
    }
}
