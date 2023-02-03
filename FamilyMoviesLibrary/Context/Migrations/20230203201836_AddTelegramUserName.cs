using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FamilyMoviesLibrary.Context.Migrations
{
    /// <inheritdoc />
    public partial class AddTelegramUserName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TelegramUserName",
                table: "Users",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TelegramUserName",
                table: "Users");
        }
    }
}
