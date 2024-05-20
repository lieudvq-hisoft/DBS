using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DBS.Migrations
{
    /// <inheritdoc />
    public partial class PriceConfiguration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PriceConfigurations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BaseFareFirst3km_Price = table.Column<long>(type: "bigint", nullable: false, defaultValue: 110000),
                    BaseFareFirst3km_IsPercent = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    FareFerAdditionalKm_Price = table.Column<long>(type: "bigint", nullable: false, defaultValue: 20000),
                    FareFerAdditionalKm_IsPercent = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DriverProfit_Price = table.Column<long>(type: "bigint", nullable: false, defaultValue: 80),
                    DriverProfit_IsPercent = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    AppProfit_Price = table.Column<long>(type: "bigint", nullable: false, defaultValue: 20),
                    AppProfit_IsPercent = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    PeakHours_Time = table.Column<string>(type: "text", nullable: false, defaultValue: "17:00-19:59"),
                    PeakHours_Price = table.Column<long>(type: "bigint", nullable: false, defaultValue: 10),
                    PeakHours_IsPercent = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    NightSurcharge_Time = table.Column<string>(type: "text", nullable: false, defaultValue: "22:00-5:59"),
                    NightSurcharge_Price = table.Column<long>(type: "bigint", nullable: false, defaultValue: 20),
                    NightSurcharge_IsPercent = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    WaitingSurcharge_PerMinutes = table.Column<int>(type: "integer", nullable: false, defaultValue: 10),
                    WaitingSurcharge_Price = table.Column<long>(type: "bigint", nullable: false, defaultValue: 20000),
                    WaitingSurcharge_IsPercent = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    WeatherFee_Price = table.Column<long>(type: "bigint", nullable: false, defaultValue: 20000),
                    WeatherFee_IsPercent = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DateCreated = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    DateUpdated = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PriceConfigurations", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PriceConfigurations");
        }
    }
}
