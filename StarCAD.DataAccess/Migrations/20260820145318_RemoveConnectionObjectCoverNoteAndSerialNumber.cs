using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StarCAD.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class RemoveConnectionObjectCoverNoteAndSerialNumber : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ConnectionObject",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "CoverNote",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "SerialNumber",
                table: "ProjectFirms");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ConnectionObject",
                table: "Projects",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CoverNote",
                table: "Projects",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SerialNumber",
                table: "ProjectFirms",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);
        }
    }
}
