using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DBS.Migrations
{
    /// <inheritdoc />
    public partial class AddField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Note",
                table: "SearchRequests",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BookingImageTime",
                table: "BookingImages",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsPublicGender",
                table: "AspNetUsers",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Note",
                table: "SearchRequests");

            migrationBuilder.DropColumn(
                name: "BookingImageTime",
                table: "BookingImages");

            migrationBuilder.DropColumn(
                name: "IsPublicGender",
                table: "AspNetUsers");
        }
    }
}
