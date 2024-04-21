using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DBS.Migrations
{
    /// <inheritdoc />
    public partial class RemoveSearchRequestDetail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SearchRequests_BookingVehicles_BookingVehicleId",
                table: "SearchRequests");

            migrationBuilder.DropTable(
                name: "BookingVehicles");

            migrationBuilder.DropTable(
                name: "SearchRequestDetails");

            migrationBuilder.DropIndex(
                name: "IX_SearchRequests_BookingVehicleId",
                table: "SearchRequests");

            migrationBuilder.DropColumn(
                name: "BookingVehicleId",
                table: "SearchRequests");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "BookingVehicleId",
                table: "SearchRequests",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "BookingVehicles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Brand = table.Column<string>(type: "text", nullable: false),
                    Color = table.Column<string>(type: "text", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    DateUpdated = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    ImageUrl = table.Column<string>(type: "text", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    LicensePlate = table.Column<string>(type: "text", nullable: false),
                    Model = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookingVehicles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SearchRequestDetails",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SearchRequestId = table.Column<Guid>(type: "uuid", nullable: false),
                    Brand = table.Column<string>(type: "text", nullable: true),
                    Color = table.Column<string>(type: "text", nullable: true),
                    DateCreated = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    DateUpdated = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    ImageUrl = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    LicensePlate = table.Column<string>(type: "text", nullable: true),
                    Name = table.Column<string>(type: "text", nullable: true),
                    Note = table.Column<string>(type: "text", nullable: true),
                    PhoneNumber = table.Column<string>(type: "text", nullable: true),
                    VehicleImage = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SearchRequestDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SearchRequestDetails_SearchRequests_SearchRequestId",
                        column: x => x.SearchRequestId,
                        principalTable: "SearchRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SearchRequests_BookingVehicleId",
                table: "SearchRequests",
                column: "BookingVehicleId");

            migrationBuilder.CreateIndex(
                name: "IX_SearchRequestDetails_SearchRequestId",
                table: "SearchRequestDetails",
                column: "SearchRequestId");

            migrationBuilder.AddForeignKey(
                name: "FK_SearchRequests_BookingVehicles_BookingVehicleId",
                table: "SearchRequests",
                column: "BookingVehicleId",
                principalTable: "BookingVehicles",
                principalColumn: "Id");
        }
    }
}
