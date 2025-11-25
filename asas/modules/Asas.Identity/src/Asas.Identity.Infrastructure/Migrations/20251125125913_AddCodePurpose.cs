using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Asas.Identity.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCodePurpose : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Purpose",
                table: "EmailConfirmationCodes",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Purpose",
                table: "EmailConfirmationCodes");
        }
    }
}
