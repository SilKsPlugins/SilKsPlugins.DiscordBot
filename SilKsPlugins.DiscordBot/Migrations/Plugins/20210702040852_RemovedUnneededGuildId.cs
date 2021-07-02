using Microsoft.EntityFrameworkCore.Migrations;

namespace SilKsPlugins.DiscordBot.Migrations.Plugins
{
    public partial class RemovedUnneededGuildId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GuildId",
                table: "Plugins");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<ulong>(
                name: "GuildId",
                table: "Plugins",
                type: "bigint unsigned",
                nullable: true);
        }
    }
}
