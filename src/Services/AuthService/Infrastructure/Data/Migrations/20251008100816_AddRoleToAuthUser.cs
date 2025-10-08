using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RateWatch.AuthService.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddRoleToAuthUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Role",
                table: "AuthUsers",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Role",
                table: "AuthUsers");
        }
    }
}
