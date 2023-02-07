using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FamilyMoviesLibrary.Context.Migrations
{
    /// <inheritdoc />
    public partial class AddGenreId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "GenreId",
                table: "Films",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GenreId",
                table: "Films");
        }
    }
}
