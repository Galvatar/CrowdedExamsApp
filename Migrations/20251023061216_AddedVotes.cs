using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace netventure.Migrations
{
    /// <inheritdoc />
    public partial class AddedVotes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Votes",
                table: "Solutions",
                newName: "UpVotes");

            migrationBuilder.AddColumn<int>(
                name: "DownVotes",
                table: "Solutions",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DownVotes",
                table: "Solutions");

            migrationBuilder.RenameColumn(
                name: "UpVotes",
                table: "Solutions",
                newName: "Votes");
        }
    }
}
