using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace SilKsPlugins.DiscordBot.Migrations.Administration
{
    public partial class NeverGenerateValue : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<ulong>(
                name: "ChannelId",
                table: "LogChannels",
                type: "bigint unsigned",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "bigint unsigned")
                .OldAnnotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<ulong>(
                name: "ChannelId",
                table: "LogChannels",
                type: "bigint unsigned",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "bigint unsigned")
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);
        }
    }
}
