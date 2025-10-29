using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace netventure.Migrations
{
    /// <inheritdoc />
    public partial class AddNewAttributesToTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "createdTime",
                table: "Solutions",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "createdTime",
                table: "Replies",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "createdTime",
                table: "Exams",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "createdTime",
                table: "Solutions");

            migrationBuilder.DropColumn(
                name: "createdTime",
                table: "Replies");

            migrationBuilder.DropColumn(
                name: "createdTime",
                table: "Exams");
        }
    }
}
