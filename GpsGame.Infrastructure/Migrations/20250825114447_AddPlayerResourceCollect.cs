using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GpsGame.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPlayerResourceCollect : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PlayerResourceCollect",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PlayerId = table.Column<Guid>(type: "uuid", nullable: false),
                    ResourceNodeId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerResourceCollect", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Collect_Player_Node_CreatedUtc",
                table: "PlayerResourceCollect",
                columns: new[] { "PlayerId", "ResourceNodeId", "CreatedUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlayerResourceCollect");
        }
    }
}
