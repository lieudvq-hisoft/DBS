using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DBS.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomerCancelFee : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "CustomerCancelFee_IsPercent",
                table: "PriceConfigurations",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "CustomerCancelFee_Price",
                table: "PriceConfigurations",
                type: "bigint",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CustomerCancelFee_IsPercent",
                table: "PriceConfigurations");

            migrationBuilder.DropColumn(
                name: "CustomerCancelFee_Price",
                table: "PriceConfigurations");
        }
    }
}
