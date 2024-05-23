using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DBS.Migrations
{
    /// <inheritdoc />
    public partial class AddLinkedAccount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "LinkedAccountId",
                table: "WalletTransactions",
                type: "uuid",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LinkedAccountId",
                table: "WalletTransactions");
        }
    }
}
