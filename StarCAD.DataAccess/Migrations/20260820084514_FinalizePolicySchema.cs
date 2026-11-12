using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StarCAD.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class FinalizePolicySchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsPaid",
                table: "Policies");

            migrationBuilder.RenameColumn(
                name: "IsPrinted",
                table: "Policies",
                newName: "IsUnitDeleted");

            migrationBuilder.AlterColumn<int>(
                name: "ProjectUnitId",
                table: "Policies",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "PolicyNumber",
                table: "Policies",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ProjectId",
                table: "Policies",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Policies_ProjectId",
                table: "Policies",
                column: "ProjectId");

            migrationBuilder.AddForeignKey(
                name: "FK_Policies_Projects_ProjectId",
                table: "Policies",
                column: "ProjectId",
                principalTable: "Projects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Policies_Projects_ProjectId",
                table: "Policies");

            migrationBuilder.DropIndex(
                name: "IX_Policies_ProjectId",
                table: "Policies");

            migrationBuilder.DropColumn(
                name: "ProjectId",
                table: "Policies");

            migrationBuilder.RenameColumn(
                name: "IsUnitDeleted",
                table: "Policies",
                newName: "IsPrinted");

            migrationBuilder.AlterColumn<int>(
                name: "ProjectUnitId",
                table: "Policies",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PolicyNumber",
                table: "Policies",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsPaid",
                table: "Policies",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
