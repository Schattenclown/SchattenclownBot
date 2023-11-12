using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SchattenclownBot.Migrations
{
    /// <inheritdoc />
    public partial class Update : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OnlineTime",
                table: "UserLevelSystems");

            migrationBuilder.DropColumn(
                name: "VoteRatingAverage",
                table: "UserLevelSystems");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<TimeSpan>(
                name: "OnlineTime",
                table: "UserLevelSystems",
                type: "time",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));

            migrationBuilder.AddColumn<double>(
                name: "VoteRatingAverage",
                table: "UserLevelSystems",
                type: "float",
                nullable: false,
                defaultValue: 0.0);
        }
    }
}
