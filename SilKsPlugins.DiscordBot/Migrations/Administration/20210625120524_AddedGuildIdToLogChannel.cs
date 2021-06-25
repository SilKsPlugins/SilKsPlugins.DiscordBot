using Microsoft.EntityFrameworkCore.Migrations;

namespace SilKsPlugins.DiscordBot.Migrations.Administration
{
    public partial class AddedGuildIdToLogChannel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_LogChannels",
                table: "LogChannels");

            migrationBuilder.AddColumn<ulong>(
                name: "GuildId",
                table: "LogChannels",
                type: "bigint unsigned",
                nullable: false,
                defaultValue: 0ul);

            migrationBuilder.AddPrimaryKey(
                name: "PK_LogChannels",
                table: "LogChannels",
                columns: new[] { "GuildId", "ChannelId" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_LogChannels",
                table: "LogChannels");

            migrationBuilder.DropColumn(
                name: "GuildId",
                table: "LogChannels");

            migrationBuilder.AddPrimaryKey(
                name: "PK_LogChannels",
                table: "LogChannels",
                column: "ChannelId");
        }
    }
}
