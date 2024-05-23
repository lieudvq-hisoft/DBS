using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DBS.Migrations
{
    /// <inheritdoc />
    public partial class AddLinkedAccountWalletTransaction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "LinkedAccountId",
                table: "WalletTransactions",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "UserModel",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    PhoneNumber = table.Column<string>(type: "text", nullable: true),
                    UserName = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    Address = table.Column<string>(type: "text", nullable: true),
                    Star = table.Column<float>(type: "real", nullable: true),
                    Avatar = table.Column<string>(type: "text", nullable: true),
                    Gender = table.Column<int>(type: "integer", nullable: true),
                    Dob = table.Column<DateOnly>(type: "date", nullable: true),
                    IsPublicGender = table.Column<bool>(type: "boolean", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserModel", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LinkedAccountModel",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    AccountNumber = table.Column<string>(type: "text", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Brand = table.Column<string>(type: "text", nullable: false),
                    LinkedImgUrl = table.Column<string>(type: "text", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LinkedAccountModel", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LinkedAccountModel_UserModel_UserId",
                        column: x => x.UserId,
                        principalTable: "UserModel",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WalletTransactions_LinkedAccountId",
                table: "WalletTransactions",
                column: "LinkedAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_LinkedAccountModel_UserId",
                table: "LinkedAccountModel",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_WalletTransactions_LinkedAccountModel_LinkedAccountId",
                table: "WalletTransactions",
                column: "LinkedAccountId",
                principalTable: "LinkedAccountModel",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WalletTransactions_LinkedAccountModel_LinkedAccountId",
                table: "WalletTransactions");

            migrationBuilder.DropTable(
                name: "LinkedAccountModel");

            migrationBuilder.DropTable(
                name: "UserModel");

            migrationBuilder.DropIndex(
                name: "IX_WalletTransactions_LinkedAccountId",
                table: "WalletTransactions");

            migrationBuilder.DropColumn(
                name: "LinkedAccountId",
                table: "WalletTransactions");
        }
    }
}
