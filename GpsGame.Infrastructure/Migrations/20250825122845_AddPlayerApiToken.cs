using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GpsGame.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPlayerApiToken : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Username",
                table: "Players",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(64)",
                oldMaxLength: 64,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ApiToken",
                table: "Players",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedUtc",
                table: "Players",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateIndex(
                name: "IX_Players_ApiToken",
                table: "Players",
                column: "ApiToken",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Players_ApiToken",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "ApiToken",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "UpdatedUtc",
                table: "Players");

            migrationBuilder.AlterColumn<string>(
                name: "Username",
                table: "Players",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(64)",
                oldMaxLength: 64);
        }
    }
}
