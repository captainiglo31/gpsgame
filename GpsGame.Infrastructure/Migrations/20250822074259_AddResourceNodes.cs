using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GpsGame.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddResourceNodes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ResourceNodes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Latitude = table.Column<double>(type: "double precision", precision: 9, scale: 6, nullable: false),
                    Longitude = table.Column<double>(type: "double precision", precision: 9, scale: 6, nullable: false),
                    Amount = table.Column<int>(type: "integer", nullable: false),
                    MaxAmount = table.Column<int>(type: "integer", nullable: false),
                    CreatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RespawnAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResourceNodes", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ResourceNodes_Latitude_Longitude",
                table: "ResourceNodes",
                columns: new[] { "Latitude", "Longitude" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ResourceNodes");
        }
    }
}
