using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace SilKsPlugins.DiscordBot.Migrations.Plugins
{
    public partial class AddedNewPluginFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreationTime",
                table: "Plugins",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IconUrl",
                table: "Plugins",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "Platforms",
                table: "Plugins",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreationTime",
                table: "Plugins");

            migrationBuilder.DropColumn(
                name: "IconUrl",
                table: "Plugins");

            migrationBuilder.DropColumn(
                name: "Platforms",
                table: "Plugins");
        }
    }
}
