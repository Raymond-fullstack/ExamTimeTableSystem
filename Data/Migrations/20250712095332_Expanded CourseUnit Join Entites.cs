using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ExamTimeTable.Data.Migrations
{
    /// <inheritdoc />
    public partial class ExpandedCourseUnitJoinEntites : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CustomCode",
                table: "SubjectCombinationCourseUnits",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "SubjectCombinationCourseUnitId",
                table: "SubjectCombinationCourseUnits",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "CustomCode",
                table: "ProgrammeCourseUnits",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "ProgrammeCourseUnitId",
                table: "ProgrammeCourseUnits",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CustomCode",
                table: "SubjectCombinationCourseUnits");

            migrationBuilder.DropColumn(
                name: "SubjectCombinationCourseUnitId",
                table: "SubjectCombinationCourseUnits");

            migrationBuilder.DropColumn(
                name: "CustomCode",
                table: "ProgrammeCourseUnits");

            migrationBuilder.DropColumn(
                name: "ProgrammeCourseUnitId",
                table: "ProgrammeCourseUnits");
        }
    }
}
