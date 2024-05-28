using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DBS.Migrations
{
    /// <inheritdoc />
    public partial class AddForeignKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_WalletTransactions_LinkedAccountId",
                table: "WalletTransactions",
                column: "LinkedAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Supports_BookingId",
                table: "Supports",
                column: "BookingId");

            migrationBuilder.AddForeignKey(
                name: "FK_Supports_Bookings_BookingId",
                table: "Supports",
                column: "BookingId",
                principalTable: "Bookings",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_WalletTransactions_LinkedAccounts_LinkedAccountId",
                table: "WalletTransactions",
                column: "LinkedAccountId",
                principalTable: "LinkedAccounts",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Supports_Bookings_BookingId",
                table: "Supports");

            migrationBuilder.DropForeignKey(
                name: "FK_WalletTransactions_LinkedAccounts_LinkedAccountId",
                table: "WalletTransactions");

            migrationBuilder.DropIndex(
                name: "IX_WalletTransactions_LinkedAccountId",
                table: "WalletTransactions");

            migrationBuilder.DropIndex(
                name: "IX_Supports_BookingId",
                table: "Supports");
        }
    }
}
