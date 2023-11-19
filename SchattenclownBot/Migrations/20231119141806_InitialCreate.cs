using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SchattenclownBot.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TwitchNotifiers",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DiscordGuildId = table.Column<decimal>(type: "decimal(20,0)", nullable: false),
                    DiscordMemberId = table.Column<decimal>(type: "decimal(20,0)", nullable: false),
                    DiscordChannelId = table.Column<decimal>(type: "decimal(20,0)", nullable: false),
                    DiscordRoleId = table.Column<decimal>(type: "decimal(20,0)", nullable: false),
                    TwitchUserId = table.Column<decimal>(type: "decimal(20,0)", nullable: false),
                    TwitchChannelUrl = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TwitchNotifiers", x => x.ID);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TwitchNotifiers");
        }
    }
}
