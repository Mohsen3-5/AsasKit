using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Asas.Identity.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUserDevice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeviceToken",
                table: "AsasUsers");

            migrationBuilder.CreateTable(
                name: "UserDevices",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    DeviceToken = table.Column<string>(type: "text", nullable: false),
                    DeviceType = table.Column<string>(type: "text", nullable: false),
                    DeviceModel = table.Column<string>(type: "text", nullable: true),
                    PlatformVersion = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserDevices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserDevices_AsasUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AsasUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserDevices_UserId_DeviceToken",
                table: "UserDevices",
                columns: new[] { "UserId", "DeviceToken" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserDevices");

            migrationBuilder.AddColumn<string>(
                name: "DeviceToken",
                table: "AsasUsers",
                type: "text",
                nullable: true);
        }
    }
}
