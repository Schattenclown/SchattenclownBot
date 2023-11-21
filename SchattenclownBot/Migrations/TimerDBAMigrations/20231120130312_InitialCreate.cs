using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace SchattenclownBot.Migrations.TimerDBAMigrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable("Timers",
                        table => new
                        {
                                    ID = table.Column<int>("int", nullable: false).Annotation("SqlServer:Identity", "1, 1"),
                                    NotificationTime = table.Column<DateTime>("datetime2", nullable: false),
                                    ChannelID = table.Column<decimal>("decimal(20,0)", nullable: false),
                                    MemberID = table.Column<decimal>("decimal(20,0)", nullable: false)
                        },
                        constraints: table =>
                        {
                            table.PrimaryKey("PK_Timers", x => x.ID);
                        });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable("Timers");
        }
    }
}