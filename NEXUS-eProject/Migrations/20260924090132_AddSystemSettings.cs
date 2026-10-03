using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NEXUS_eProject.Migrations
{
    /// <inheritdoc />
    public partial class AddSystemSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SystemSettings",
                columns: table => new
                {
                    SystemSettingId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ApplicationName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SupportEmail = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    SupportPhone = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    CompanyAddress = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    TaxPercentage = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    DefaultBillDueDays = table.Column<int>(type: "int", nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    CityCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    BroadbandPrefix = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: false),
                    TelephonePrefix = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: false),
                    DialUpPrefix = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: false),
                    CustomerRegistrationEnabled = table.Column<bool>(type: "bit", nullable: false),
                    FeedbackEnabled = table.Column<bool>(type: "bit", nullable: false),
                    MaintenanceMode = table.Column<bool>(type: "bit", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemSettings", x => x.SystemSettingId);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SystemSettings");
        }
    }
}
