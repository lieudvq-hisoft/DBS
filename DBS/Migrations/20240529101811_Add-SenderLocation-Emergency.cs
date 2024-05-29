using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DBS.Migrations
{
    /// <inheritdoc />
    public partial class AddSenderLocationEmergency : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "SenderLatitude",
                table: "Emergencies",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "SenderLongitude",
                table: "Emergencies",
                type: "double precision",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SenderLatitude",
                table: "Emergencies");

            migrationBuilder.DropColumn(
                name: "SenderLongitude",
                table: "Emergencies");
        }
    }
}
