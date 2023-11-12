using System;
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
                name: "UserLevelSystems",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DiscordMemberID = table.Column<decimal>(type: "decimal(20,0)", nullable: false),
                    DiscordGuildID = table.Column<decimal>(type: "decimal(20,0)", nullable: false),
                    OnlineTicks = table.Column<int>(type: "int", nullable: false),
                    OnlineTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    VoteRatingAverage = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserLevelSystems", x => x.ID);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserLevelSystems");
        }
    }
}
