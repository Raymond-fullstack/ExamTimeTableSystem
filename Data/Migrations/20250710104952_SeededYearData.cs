using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ExamTimeTable.Data.Migrations
{
    /// <inheritdoc />
    public partial class SeededYearData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Years",
                columns: new[] { "YearId", "Name" },
                values: new object[,]
                {
                    { 1, "HEAC" },
                    { 2, "Year 1" },
                    { 3, "Year 2" },
                    { 4, "Year 3" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Years",
                keyColumn: "YearId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Years",
                keyColumn: "YearId",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Years",
                keyColumn: "YearId",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Years",
                keyColumn: "YearId",
                keyValue: 4);
        }
    }
}
