using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DBS.Migrations
{
    /// <inheritdoc />
    public partial class updateBooking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_SearchRequests_SearchRequestId",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "SeachRequestId",
                table: "Bookings");

            migrationBuilder.AlterColumn<Guid>(
                name: "SearchRequestId",
                table: "Bookings",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_SearchRequests_SearchRequestId",
                table: "Bookings",
                column: "SearchRequestId",
                principalTable: "SearchRequests",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_SearchRequests_SearchRequestId",
                table: "Bookings");

            migrationBuilder.AlterColumn<Guid>(
                name: "SearchRequestId",
                table: "Bookings",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<Guid>(
                name: "SeachRequestId",
                table: "Bookings",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_SearchRequests_SearchRequestId",
                table: "Bookings",
                column: "SearchRequestId",
                principalTable: "SearchRequests",
                principalColumn: "Id");
        }
    }
}
