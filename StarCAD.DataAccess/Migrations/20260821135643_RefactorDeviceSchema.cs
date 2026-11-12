using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StarCAD.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class RefactorDeviceSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Devices_Codes_DeviceTypeCodeId",
                table: "Devices");

            migrationBuilder.DropIndex(
                name: "IX_Devices_DeviceTypeCodeId",
                table: "Devices");

            migrationBuilder.DropColumn(
                name: "DeviceTypeCodeId",
                table: "Devices");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Devices");

            migrationBuilder.AddColumn<string>(
                name: "Brand",
                table: "Devices",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Capacity",
                table: "Devices",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeviceType",
                table: "Devices",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "FlowCubicMeterPerHour",
                table: "Devices",
                type: "decimal(10,2)",
                precision: 10,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FlueLabel",
                table: "Devices",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Model",
                table: "Devices",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Brand",
                table: "Devices");

            migrationBuilder.DropColumn(
                name: "Capacity",
                table: "Devices");

            migrationBuilder.DropColumn(
                name: "DeviceType",
                table: "Devices");

            migrationBuilder.DropColumn(
                name: "FlowCubicMeterPerHour",
                table: "Devices");

            migrationBuilder.DropColumn(
                name: "FlueLabel",
                table: "Devices");

            migrationBuilder.DropColumn(
                name: "Model",
                table: "Devices");

            migrationBuilder.AddColumn<int>(
                name: "DeviceTypeCodeId",
                table: "Devices",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Devices",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_Devices_DeviceTypeCodeId",
                table: "Devices",
                column: "DeviceTypeCodeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Devices_Codes_DeviceTypeCodeId",
                table: "Devices",
                column: "DeviceTypeCodeId",
                principalTable: "Codes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
