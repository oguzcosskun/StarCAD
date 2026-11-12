using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace StarCAD.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class SeedInsuranceCompanies : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "InsuranceCompanies",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.InsertData(
                table: "InsuranceCompanies",
                columns: new[] { "Id", "IsActive", "Title" },
                values: new object[,]
                {
                    { 1, true, "Anadolu Sigorta" },
                    { 2, true, "Allianz Sigorta" },
                    { 3, true, "Aksigorta" },
                    { 4, true, "AXA Sigorta" },
                    { 5, true, "Türkiye Sigorta" },
                    { 6, true, "HDI Sigorta" },
                    { 7, true, "Ray Sigorta" },
                    { 8, true, "Sompo Sigorta" },
                    { 9, true, "Zurich Sigorta" },
                    { 10, true, "Mapfre Sigorta" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "InsuranceCompanies",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "InsuranceCompanies",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "InsuranceCompanies",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "InsuranceCompanies",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "InsuranceCompanies",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "InsuranceCompanies",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "InsuranceCompanies",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "InsuranceCompanies",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "InsuranceCompanies",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "InsuranceCompanies",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "InsuranceCompanies",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);
        }
    }
}
