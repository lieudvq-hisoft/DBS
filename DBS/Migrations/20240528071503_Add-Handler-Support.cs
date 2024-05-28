using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DBS.Migrations
{
    /// <inheritdoc />
    public partial class AddHandlerSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "HandlerId",
                table: "Supports",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Supports_HandlerId",
                table: "Supports",
                column: "HandlerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Supports_AspNetUsers_HandlerId",
                table: "Supports",
                column: "HandlerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Supports_AspNetUsers_HandlerId",
                table: "Supports");

            migrationBuilder.DropIndex(
                name: "IX_Supports_HandlerId",
                table: "Supports");

            migrationBuilder.DropColumn(
                name: "HandlerId",
                table: "Supports");
        }
    }
}
