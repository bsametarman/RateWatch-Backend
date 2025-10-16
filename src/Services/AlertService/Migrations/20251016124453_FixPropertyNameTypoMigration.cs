using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RateWatch.AlertService.Migrations
{
    /// <inheritdoc />
    public partial class FixPropertyNameTypoMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Treshold",
                table: "Alerts",
                newName: "Threshold");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Threshold",
                table: "Alerts",
                newName: "Treshold");
        }
    }
}
