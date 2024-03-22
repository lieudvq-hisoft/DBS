using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DBS.Migrations
{
    /// <inheritdoc />
    public partial class IdentityCard : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "IdentityCards",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FullName = table.Column<string>(type: "text", nullable: true),
                    Dob = table.Column<DateOnly>(type: "date", nullable: false),
                    Gender = table.Column<int>(type: "integer", nullable: false),
                    Nationality = table.Column<string>(type: "text", nullable: true),
                    PlaceOrigin = table.Column<string>(type: "text", nullable: true),
                    PlaceResidence = table.Column<string>(type: "text", nullable: true),
                    PersonalIdentification = table.Column<string>(type: "text", nullable: true),
                    ExpiredDate = table.Column<DateOnly>(type: "date", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    DateUpdated = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IdentityCards", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "IdentityCardImages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    IdentityCardId = table.Column<Guid>(type: "uuid", nullable: false),
                    ImageData = table.Column<string>(type: "text", nullable: true),
                    IsFront = table.Column<bool>(type: "boolean", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    DateUpdated = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IdentityCardImages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IdentityCardImages_IdentityCards_IdentityCardId",
                        column: x => x.IdentityCardId,
                        principalTable: "IdentityCards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_IdentityCardImages_IdentityCardId",
                table: "IdentityCardImages",
                column: "IdentityCardId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IdentityCardImages");

            migrationBuilder.DropTable(
                name: "IdentityCards");
        }
    }
}
