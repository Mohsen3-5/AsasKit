using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Asas.Tenancy.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class editModuleName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "Tenants",
                type: "uniqueidentifier",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Tenants");
        }
    }
}
