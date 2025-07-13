using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ExamTimeTable.Data.Migrations
{
    /// <inheritdoc />
    public partial class YearDepartmentChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Years_Departments_DepartmentId",
                table: "Years");

            migrationBuilder.DropIndex(
                name: "IX_Years_DepartmentId",
                table: "Years");

            migrationBuilder.DropColumn(
                name: "DepartmentId",
                table: "Years");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DepartmentId",
                table: "Years",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Years_DepartmentId",
                table: "Years",
                column: "DepartmentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Years_Departments_DepartmentId",
                table: "Years",
                column: "DepartmentId",
                principalTable: "Departments",
                principalColumn: "DepartmentId");
        }
    }
}
