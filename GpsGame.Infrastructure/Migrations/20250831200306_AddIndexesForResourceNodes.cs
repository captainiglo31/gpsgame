using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GpsGame.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddIndexesForResourceNodes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_ResourceNodes_Amount_RespawnAtUtc",
                table: "ResourceNodes",
                columns: new[] { "Amount", "RespawnAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_ResourceNodes_RespawnAtUtc",
                table: "ResourceNodes",
                column: "RespawnAtUtc");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ResourceNodes_Amount_RespawnAtUtc",
                table: "ResourceNodes");

            migrationBuilder.DropIndex(
                name: "IX_ResourceNodes_RespawnAtUtc",
                table: "ResourceNodes");
        }
    }
}
