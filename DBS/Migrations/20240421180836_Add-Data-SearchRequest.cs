using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DBS.Migrations
{
    /// <inheritdoc />
    public partial class AddDataSearchRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "BookingVehicleId",
                table: "SearchRequests",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CustomerBookedOnBehalfId",
                table: "SearchRequests",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "BookingVehicles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LicensePlate = table.Column<string>(type: "text", nullable: false),
                    Brand = table.Column<string>(type: "text", nullable: false),
                    Model = table.Column<string>(type: "text", nullable: false),
                    Color = table.Column<string>(type: "text", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    DateUpdated = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookingVehicles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CustomerBookedOnBehalves",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: true),
                    PhoneNumber = table.Column<string>(type: "text", nullable: true),
                    Note = table.Column<string>(type: "text", nullable: true),
                    DateCreated = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    DateUpdated = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerBookedOnBehalves", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SearchRequests_BookingVehicleId",
                table: "SearchRequests",
                column: "BookingVehicleId");

            migrationBuilder.CreateIndex(
                name: "IX_SearchRequests_CustomerBookedOnBehalfId",
                table: "SearchRequests",
                column: "CustomerBookedOnBehalfId");

            migrationBuilder.AddForeignKey(
                name: "FK_SearchRequests_BookingVehicles_BookingVehicleId",
                table: "SearchRequests",
                column: "BookingVehicleId",
                principalTable: "BookingVehicles",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SearchRequests_CustomerBookedOnBehalves_CustomerBookedOnBeh~",
                table: "SearchRequests",
                column: "CustomerBookedOnBehalfId",
                principalTable: "CustomerBookedOnBehalves",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SearchRequests_BookingVehicles_BookingVehicleId",
                table: "SearchRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_SearchRequests_CustomerBookedOnBehalves_CustomerBookedOnBeh~",
                table: "SearchRequests");

            migrationBuilder.DropTable(
                name: "BookingVehicles");

            migrationBuilder.DropTable(
                name: "CustomerBookedOnBehalves");

            migrationBuilder.DropIndex(
                name: "IX_SearchRequests_BookingVehicleId",
                table: "SearchRequests");

            migrationBuilder.DropIndex(
                name: "IX_SearchRequests_CustomerBookedOnBehalfId",
                table: "SearchRequests");

            migrationBuilder.DropColumn(
                name: "BookingVehicleId",
                table: "SearchRequests");

            migrationBuilder.DropColumn(
                name: "CustomerBookedOnBehalfId",
                table: "SearchRequests");
        }
    }
}
