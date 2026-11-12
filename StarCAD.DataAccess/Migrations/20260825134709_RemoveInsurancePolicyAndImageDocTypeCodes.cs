using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace StarCAD.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class RemoveInsurancePolicyAndImageDocTypeCodes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Codes",
                keyColumn: "Id",
                keyValue: 5017);

            migrationBuilder.DeleteData(
                table: "Codes",
                keyColumn: "Id",
                keyValue: 5018);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Codes",
                columns: new[] { "Id", "CodeValue", "GroupId", "IsActive", "Name" },
                values: new object[,]
                {
                    { 5017, "InsurancePolicy", 5, true, "Poliçe" },
                    { 5018, "Image", 5, true, "Resim" }
                });
        }
    }
}
