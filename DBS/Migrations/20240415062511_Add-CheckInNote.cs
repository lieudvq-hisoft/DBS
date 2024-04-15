using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DBS.Migrations
{
    /// <inheritdoc />
    public partial class AddCheckInNote : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ExpriedDate",
                table: "DrivingLicenses",
                newName: "ExpiredDate");

            migrationBuilder.AddColumn<int>(
                name: "BookingType",
                table: "SearchRequests",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "DrivingLicenseNumber",
                table: "DrivingLicenses",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CheckInNote",
                table: "Bookings",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BookingType",
                table: "SearchRequests");

            migrationBuilder.DropColumn(
                name: "DrivingLicenseNumber",
                table: "DrivingLicenses");

            migrationBuilder.DropColumn(
                name: "CheckInNote",
                table: "Bookings");

            migrationBuilder.RenameColumn(
                name: "ExpiredDate",
                table: "DrivingLicenses",
                newName: "ExpriedDate");
        }
    }
}
