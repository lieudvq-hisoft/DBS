using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DBS.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSearchRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PickupLocation",
                table: "SearchRequests",
                newName: "PickupLongitude");

            migrationBuilder.RenameColumn(
                name: "DropOffLocation",
                table: "SearchRequests",
                newName: "PickupLatitude");

            migrationBuilder.AddColumn<double>(
                name: "DropOffLatitude",
                table: "SearchRequests",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "DropOffLongitude",
                table: "SearchRequests",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DropOffLatitude",
                table: "SearchRequests");

            migrationBuilder.DropColumn(
                name: "DropOffLongitude",
                table: "SearchRequests");

            migrationBuilder.RenameColumn(
                name: "PickupLongitude",
                table: "SearchRequests",
                newName: "PickupLocation");

            migrationBuilder.RenameColumn(
                name: "PickupLatitude",
                table: "SearchRequests",
                newName: "DropOffLocation");
        }
    }
}
