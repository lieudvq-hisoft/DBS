using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DBS.Migrations
{
    /// <inheritdoc />
    public partial class BookingCancel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BookingCancels",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BookingId = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelPersonId = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelReason = table.Column<string>(type: "text", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    DateUpdated = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookingCancels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BookingCancels_AspNetUsers_CancelPersonId",
                        column: x => x.CancelPersonId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BookingCancels_Bookings_BookingId",
                        column: x => x.BookingId,
                        principalTable: "Bookings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BookingCancelImages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BookingCancelId = table.Column<Guid>(type: "uuid", nullable: false),
                    ImageUrl = table.Column<string>(type: "text", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    DateUpdated = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookingCancelImages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BookingCancelImages_BookingCancels_BookingCancelId",
                        column: x => x.BookingCancelId,
                        principalTable: "BookingCancels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BookingCancelImages_BookingCancelId",
                table: "BookingCancelImages",
                column: "BookingCancelId");

            migrationBuilder.CreateIndex(
                name: "IX_BookingCancels_BookingId",
                table: "BookingCancels",
                column: "BookingId");

            migrationBuilder.CreateIndex(
                name: "IX_BookingCancels_CancelPersonId",
                table: "BookingCancels",
                column: "CancelPersonId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BookingCancelImages");

            migrationBuilder.DropTable(
                name: "BookingCancels");
        }
    }
}
