using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DBS.Migrations
{
    /// <inheritdoc />
    public partial class AddBookedPersonInfo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "BookedPersonInfoId",
                table: "SearchRequests",
                type: "uuid",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Model",
                table: "BookingVehicles",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "LicensePlate",
                table: "BookingVehicles",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Color",
                table: "BookingVehicles",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Brand",
                table: "BookingVehicles",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "BookingVehicles",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "BookedPersonInfos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: true),
                    PhoneNumber = table.Column<string>(type: "text", nullable: true),
                    ImageUrl = table.Column<string>(type: "text", nullable: true),
                    DateCreated = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    DateUpdated = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookedPersonInfos", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SearchRequests_BookedPersonInfoId",
                table: "SearchRequests",
                column: "BookedPersonInfoId");

            migrationBuilder.AddForeignKey(
                name: "FK_SearchRequests_BookedPersonInfos_BookedPersonInfoId",
                table: "SearchRequests",
                column: "BookedPersonInfoId",
                principalTable: "BookedPersonInfos",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SearchRequests_BookedPersonInfos_BookedPersonInfoId",
                table: "SearchRequests");

            migrationBuilder.DropTable(
                name: "BookedPersonInfos");

            migrationBuilder.DropIndex(
                name: "IX_SearchRequests_BookedPersonInfoId",
                table: "SearchRequests");

            migrationBuilder.DropColumn(
                name: "BookedPersonInfoId",
                table: "SearchRequests");

            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "BookingVehicles");

            migrationBuilder.AlterColumn<string>(
                name: "Model",
                table: "BookingVehicles",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "LicensePlate",
                table: "BookingVehicles",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Color",
                table: "BookingVehicles",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Brand",
                table: "BookingVehicles",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);
        }
    }
}
