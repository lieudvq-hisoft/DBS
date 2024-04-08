using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DBS.Migrations
{
    /// <inheritdoc />
    public partial class BookingVehicle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_IdentityCards_IdentityCardId",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_IdentityCardId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Brand",
                table: "SearchRequests");

            migrationBuilder.DropColumn(
                name: "Color",
                table: "SearchRequests");

            migrationBuilder.DropColumn(
                name: "LicensePlate",
                table: "SearchRequests");

            migrationBuilder.DropColumn(
                name: "Model",
                table: "SearchRequests");

            migrationBuilder.DropColumn(
                name: "IdentityCardId",
                table: "AspNetUsers");

            migrationBuilder.AddColumn<Guid>(
                name: "BookingVehicleId",
                table: "SearchRequests",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "IdentityCards",
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

            migrationBuilder.CreateIndex(
                name: "IX_SearchRequests_BookingVehicleId",
                table: "SearchRequests",
                column: "BookingVehicleId");

            migrationBuilder.CreateIndex(
                name: "IX_IdentityCards_UserId",
                table: "IdentityCards",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_IdentityCards_AspNetUsers_UserId",
                table: "IdentityCards",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SearchRequests_BookingVehicles_BookingVehicleId",
                table: "SearchRequests",
                column: "BookingVehicleId",
                principalTable: "BookingVehicles",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_IdentityCards_AspNetUsers_UserId",
                table: "IdentityCards");

            migrationBuilder.DropForeignKey(
                name: "FK_SearchRequests_BookingVehicles_BookingVehicleId",
                table: "SearchRequests");

            migrationBuilder.DropTable(
                name: "BookingVehicles");

            migrationBuilder.DropIndex(
                name: "IX_SearchRequests_BookingVehicleId",
                table: "SearchRequests");

            migrationBuilder.DropIndex(
                name: "IX_IdentityCards_UserId",
                table: "IdentityCards");

            migrationBuilder.DropColumn(
                name: "BookingVehicleId",
                table: "SearchRequests");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "IdentityCards");

            migrationBuilder.AddColumn<string>(
                name: "Brand",
                table: "SearchRequests",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Color",
                table: "SearchRequests",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "LicensePlate",
                table: "SearchRequests",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Model",
                table: "SearchRequests",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "IdentityCardId",
                table: "AspNetUsers",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_IdentityCardId",
                table: "AspNetUsers",
                column: "IdentityCardId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_IdentityCards_IdentityCardId",
                table: "AspNetUsers",
                column: "IdentityCardId",
                principalTable: "IdentityCards",
                principalColumn: "Id");
        }
    }
}
