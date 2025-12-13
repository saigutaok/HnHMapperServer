using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HnHMapperServer.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddOverlayOffsets : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OverlayOffsets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TenantId = table.Column<string>(type: "TEXT", nullable: false),
                    CurrentMapId = table.Column<int>(type: "INTEGER", nullable: false),
                    OverlayMapId = table.Column<int>(type: "INTEGER", nullable: false),
                    OffsetX = table.Column<double>(type: "REAL", nullable: false),
                    OffsetY = table.Column<double>(type: "REAL", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OverlayOffsets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OverlayOffsets_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OverlayOffsets_TenantId",
                table: "OverlayOffsets",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_OverlayOffsets_TenantId_CurrentMapId_OverlayMapId",
                table: "OverlayOffsets",
                columns: new[] { "TenantId", "CurrentMapId", "OverlayMapId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OverlayOffsets");
        }
    }
}
