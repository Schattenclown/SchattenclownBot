using Microsoft.EntityFrameworkCore.Migrations;

namespace SchattenclownBot.Migrations.SympathySystemDBAMigrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable("SympathySystems",
                        table => new
                        {
                                    ID = table.Column<int>("int", nullable: false).Annotation("SqlServer:Identity", "1, 1"),
                                    VotingMemberID = table.Column<decimal>("decimal(20,0)", nullable: false),
                                    TargetMemberID = table.Column<decimal>("decimal(20,0)", nullable: false),
                                    GuildID = table.Column<decimal>("decimal(20,0)", nullable: false),
                                    Rating = table.Column<int>("int", nullable: false)
                        },
                        constraints: table =>
                        {
                            table.PrimaryKey("PK_SympathySystems", x => x.ID);
                        });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable("SympathySystems");
        }
    }
}