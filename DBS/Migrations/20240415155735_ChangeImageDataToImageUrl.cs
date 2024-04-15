using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DBS.Migrations
{
    /// <inheritdoc />
    public partial class ChangeImageDataToImageUrl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ImageData",
                table: "VehicleImages",
                newName: "ImageUrl");

            migrationBuilder.RenameColumn(
                name: "ImageData",
                table: "SearchRequestDetails",
                newName: "ImageUrl");

            migrationBuilder.RenameColumn(
                name: "ImageData",
                table: "Ratings",
                newName: "ImageUrl");

            migrationBuilder.RenameColumn(
                name: "ImageData",
                table: "IdentityCardImages",
                newName: "ImageUrl");

            migrationBuilder.RenameColumn(
                name: "ImageData",
                table: "DrivingLicenseImages",
                newName: "ImageUrl");

            migrationBuilder.RenameColumn(
                name: "ImageData",
                table: "BookingImages",
                newName: "ImageUrl");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ImageUrl",
                table: "VehicleImages",
                newName: "ImageData");

            migrationBuilder.RenameColumn(
                name: "ImageUrl",
                table: "SearchRequestDetails",
                newName: "ImageData");

            migrationBuilder.RenameColumn(
                name: "ImageUrl",
                table: "Ratings",
                newName: "ImageData");

            migrationBuilder.RenameColumn(
                name: "ImageUrl",
                table: "IdentityCardImages",
                newName: "ImageData");

            migrationBuilder.RenameColumn(
                name: "ImageUrl",
                table: "DrivingLicenseImages",
                newName: "ImageData");

            migrationBuilder.RenameColumn(
                name: "ImageUrl",
                table: "BookingImages",
                newName: "ImageData");
        }
    }
}
